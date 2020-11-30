using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.WebChat.Annotations;
using NCS.DSS.WebChat.Cosmos.Helper;
using NCS.DSS.WebChat.Helpers;
using NCS.DSS.WebChat.PostWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.WebChat.PostWebChatHttpTrigger.Function
{
    public class PostWebChatHttpTrigger
    {
        private IResourceHelper _resourceHelper;
        private IHttpRequestHelper _httpRequestMessageHelper;
        private IValidate _validate;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private IPostWebChatHttpTriggerService _webChatPostService;

        public PostWebChatHttpTrigger(IResourceHelper resourceHelper,
            IHttpRequestHelper httpRequestMessageHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IJsonHelper jsonHelper,
            IValidate validate,
            IPostWebChatHttpTriggerService webChatPostService)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _webChatPostService = webChatPostService;
            _jsonHelper = jsonHelper;
            _validate = validate;
        }

        [FunctionName("Post")]
        [ProducesResponseTypeAttribute(typeof(Models.WebChat), (int)HttpStatusCode.Created)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "WebChat Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "WebChat does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "WebChat validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new webchat resource.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/{interactionId}/WebChats")] HttpRequest req, ILogger log, string customerId, string interactionId)
        {
            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return _httpResponseMessageHelper.BadRequest();
            }

            var ApimURL = _httpRequestMessageHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Post Web Chat C# HTTP trigger function processed a request. By Touchpoint. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return _httpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return _httpResponseMessageHelper.BadRequest(interactionGuid);

            Models.WebChat webChatRequest;

            try
            {
                webChatRequest = await _httpRequestMessageHelper.GetResourceFromRequest<Models.WebChat>(req);
            }
            catch (JsonException ex)
            {
                return _httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (webChatRequest == null)
                return _httpResponseMessageHelper.UnprocessableEntity(req);

            webChatRequest.SetIds(customerGuid, interactionGuid, touchpointId);

            var errors = _validate.ValidateResource(webChatRequest, true);

            if (errors != null && errors.Any())
                return _httpResponseMessageHelper.UnprocessableEntity(errors);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return _httpResponseMessageHelper.NoContent(customerGuid);

            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return _httpResponseMessageHelper.Forbidden(customerGuid);

            var doesInteractionExist = _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
                return _httpResponseMessageHelper.NoContent(interactionGuid);

            var webChat = await _webChatPostService.CreateAsync(webChatRequest);

            if (webChat != null)
                await _webChatPostService.SendToServiceBusQueueAsync(webChat, ApimURL);

            return webChat == null
                ? _httpResponseMessageHelper.BadRequest(customerGuid)
                : _httpResponseMessageHelper.Created(_jsonHelper.SerializeObjectAndRenameIdProperty(webChat, "id", "webChatId"));
        }
    }
}