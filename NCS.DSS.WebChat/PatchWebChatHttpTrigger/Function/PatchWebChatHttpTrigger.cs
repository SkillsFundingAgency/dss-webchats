using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.WebChat.Annotations;
using NCS.DSS.WebChat.Cosmos.Helper;
using NCS.DSS.WebChat.Helpers;
using NCS.DSS.WebChat.Models;
using NCS.DSS.WebChat.PatchWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.WebChat.PatchWebChatHttpTrigger.Function
{
    public class PatchWebChatHttpTrigger
    {
        private IResourceHelper _resourceHelper;
        private IHttpRequestMessageHelper _httpRequestMessageHelper;
        private IPatchWebChatHttpTriggerService _webChatPatchService;
        private IValidate _validate;

        public PatchWebChatHttpTrigger(IResourceHelper resourceHelper,
        IHttpRequestMessageHelper httpRequestMessageHelper,
        IValidate validate,
        IPatchWebChatHttpTriggerService webChatPatchService)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _webChatPatchService = webChatPatchService;
            _validate = validate;
        }

        [FunctionName("Patch")]
        [ResponseType(typeof(Models.WebChat))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "WebChat Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "WebChat does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "WebChat validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update an webchat record.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/WebChats/{webChatId}")]HttpRequestMessage req, ILogger log, string customerId, string interactionId, string webChatId)
        {
            var touchpointId = _httpRequestMessageHelper.GetTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return HttpResponseMessageHelper.BadRequest();
            }

            var ApimURL = _httpRequestMessageHelper.GetApimURL(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return HttpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Patch Web Chat C# HTTP trigger function processed a request. By Touchpoint. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(webChatId, out var webChatGuid))
                return HttpResponseMessageHelper.BadRequest(webChatGuid);

            WebChatPatch webChatPatchRequest;

            try
            {
                webChatPatchRequest = await _httpRequestMessageHelper.GetWebChatFromRequest<Models.WebChatPatch>(req);
            }
            catch (JsonException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (webChatPatchRequest == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);

            webChatPatchRequest.LastModifiedTouchpointId = touchpointId;
            webChatPatchRequest.SetDefaultValues();

            var errors = _validate.ValidateResource(webChatPatchRequest, false);

            if (errors != null && errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity(errors);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return HttpResponseMessageHelper.Forbidden(customerGuid);

            var doesInteractionExist = _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var webChat = await _webChatPatchService.GetWebChatForCustomerAsync(customerGuid, interactionGuid, webChatGuid);

            if (webChat == null)
                return HttpResponseMessageHelper.NoContent(webChatGuid);

            var updatedWebChat = await _webChatPatchService.UpdateAsync(webChat, webChatPatchRequest);

            if (updatedWebChat != null)
                await _webChatPatchService.SendToServiceBusQueueAsync(updatedWebChat, customerGuid, ApimURL);

            return updatedWebChat == null ?
                HttpResponseMessageHelper.BadRequest(webChatGuid) :
                HttpResponseMessageHelper.Ok(JsonHelper.SerializeObject(updatedWebChat));
        }
    }
}