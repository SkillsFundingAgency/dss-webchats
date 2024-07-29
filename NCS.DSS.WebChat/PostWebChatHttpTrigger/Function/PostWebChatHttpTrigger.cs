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
using Microsoft.Extensions.Logging;
using NCS.DSS.WebChat.Cosmos.Helper;
using DFC.Swagger.Standard.Annotations;
using NCS.DSS.WebChat.PostWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.Validation;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;

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
        private ILogger log;

        public PostWebChatHttpTrigger(IResourceHelper resourceHelper,
            IHttpRequestHelper httpRequestMessageHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IJsonHelper jsonHelper,
            IValidate validate,
            IPostWebChatHttpTriggerService webChatPostService,
            ILogger<PostWebChatHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _webChatPostService = webChatPostService;
            _jsonHelper = jsonHelper;
            _validate = validate;
            log = logger;
        }

        [Function("Post")]
        [ProducesResponseType(typeof(Models.WebChat), 201)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "WebChat Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "WebChat does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "WebChat validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new webchat resource.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/{interactionId}/WebChats")] HttpRequest req, string customerId, string interactionId)
        {
            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            var ApimURL = _httpRequestMessageHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            log.LogInformation("Post Web Chat C# HTTP trigger function processed a request. By Touchpoint. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return new BadRequestObjectResult(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return new BadRequestObjectResult(interactionGuid);

            Models.WebChat webChatRequest;

            try
            {
                webChatRequest = await _httpRequestMessageHelper.GetResourceFromRequest<Models.WebChat>(req);
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                return new UnprocessableEntityObjectResult(ex);
            }

            if (webChatRequest == null)
                return new UnprocessableEntityObjectResult(req);

            webChatRequest.SetIds(customerGuid, interactionGuid, touchpointId);

            var errors = _validate.ValidateResource(webChatRequest, true);

            if (errors != null && errors.Any())
                return new UnprocessableEntityObjectResult(errors);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return new NoContentResult();

            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return new ForbidResult();

            var doesInteractionExist = _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
                return new NoContentResult();

            var webChat = await _webChatPostService.CreateAsync(webChatRequest);

            if (webChat != null)
                await _webChatPostService.SendToServiceBusQueueAsync(webChat, ApimURL);

            return webChat == null
                ? new BadRequestObjectResult(customerGuid)
                : new JsonResult(webChat, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.Created
                };
        }
    }
}