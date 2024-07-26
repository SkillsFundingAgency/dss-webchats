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
using NCS.DSS.WebChat.Models;
using NCS.DSS.WebChat.PatchWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.Validation;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;

namespace NCS.DSS.WebChat.PatchWebChatHttpTrigger.Function
{
    public class PatchWebChatHttpTrigger
    {
        private IResourceHelper _resourceHelper;
        private IHttpRequestHelper _httpRequestMessageHelper;
        private IPatchWebChatHttpTriggerService _webChatPatchService;
        private IValidate _validate;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;

        public PatchWebChatHttpTrigger(IResourceHelper resourceHelper,
        IHttpRequestHelper httpRequestMessageHelper,
        IHttpResponseMessageHelper httpResponseMessageHelper,
        IJsonHelper jsonHelper,
        IValidate validate,
        IPatchWebChatHttpTriggerService webChatPatchService)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _webChatPatchService = webChatPatchService;
            _jsonHelper = jsonHelper;
            _validate = validate;
        }

        [Function("Patch")]
        [ProducesResponseType(typeof(Models.WebChat), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "WebChat Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "WebChat does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "WebChat validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update an webchat record.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/WebChats/{webChatId}")] HttpRequest req, ILogger log, string customerId, string interactionId, string webChatId)
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

            log.LogInformation("Patch Web Chat C# HTTP trigger function processed a request. By Touchpoint. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return _httpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return _httpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(webChatId, out var webChatGuid))
                return _httpResponseMessageHelper.BadRequest(webChatGuid);

            WebChatPatch webChatPatchRequest;

            try
            {
                webChatPatchRequest = await _httpRequestMessageHelper.GetResourceFromRequest<Models.WebChatPatch>(req);
            }
            catch (JsonException ex)
            {
                return _httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (webChatPatchRequest == null)
                return _httpResponseMessageHelper.UnprocessableEntity(req);

            webChatPatchRequest.LastModifiedTouchpointId = touchpointId;
            webChatPatchRequest.SetDefaultValues();

            var errors = _validate.ValidateResource(webChatPatchRequest, false);

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

            var webChat = await _webChatPatchService.GetWebChatForCustomerAsync(customerGuid, interactionGuid, webChatGuid);

            if (webChat == null)
                return _httpResponseMessageHelper.NoContent(webChatGuid);

            var updatedWebChat = await _webChatPatchService.UpdateAsync(webChat, webChatPatchRequest);

            if (updatedWebChat != null)
                await _webChatPatchService.SendToServiceBusQueueAsync(updatedWebChat, customerGuid, ApimURL);

            return updatedWebChat == null ?
                _httpResponseMessageHelper.BadRequest(webChatGuid) :
                _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(updatedWebChat, "id", "WebChatId"));
        }
    }
}