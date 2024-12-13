using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.WebChat.Cosmos.Provider;
using NCS.DSS.WebChat.Helpers;
using NCS.DSS.WebChat.Models;
using NCS.DSS.WebChat.PatchWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.WebChat.PatchWebChatHttpTrigger.Function
{
    public class PatchWebChatHttpTrigger
    {
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly IPatchWebChatHttpTriggerService _webChatPatchService;
        private readonly IValidate _validate;
        private readonly ILogger<PatchWebChatHttpTrigger> _logger;
        private readonly IDynamicHelper _dynamicHelper;

        public PatchWebChatHttpTrigger(ICosmosDBProvider cosmosDbProvider,
        IHttpRequestHelper httpRequestMessageHelper,
        IValidate validate,
        IPatchWebChatHttpTriggerService webChatPatchService,
        ILogger<PatchWebChatHttpTrigger> logger,
        IDynamicHelper dynamicHelper)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _webChatPatchService = webChatPatchService;
            _validate = validate;
            _logger = logger;
            _dynamicHelper = dynamicHelper;
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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/WebChats/{webChatId}")] HttpRequest req, string customerId, string interactionId, string webChatId)
        {
            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            var ApimURL = _httpRequestMessageHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                _logger.LogInformation("Unable to locate 'apimurl' in request header");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            _logger.LogInformation("Patch Web Chat C# HTTP trigger function processed a request. By Touchpoint. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return new BadRequestObjectResult(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return new BadRequestObjectResult(interactionGuid);

            if (!Guid.TryParse(webChatId, out var webChatGuid))
                return new BadRequestObjectResult(webChatGuid);

            WebChatPatch webChatPatchRequest;

            try
            {
                webChatPatchRequest = await _httpRequestMessageHelper.GetResourceFromRequest<Models.WebChatPatch>(req);
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, ["TargetSite"]));
            }

            if (webChatPatchRequest == null)
                return new UnprocessableEntityObjectResult(req);

            webChatPatchRequest.LastModifiedTouchpointId = touchpointId;
            webChatPatchRequest.SetDefaultValues();

            var errors = _validate.ValidateResource(webChatPatchRequest, false);

            if (errors != null && errors.Any())
                return new UnprocessableEntityObjectResult(errors);

            var doesCustomerExist = await _cosmosDbProvider.DoesCustomerResourceExist(customerGuid);

            if (!doesCustomerExist)
                return new NoContentResult();

            var isCustomerReadOnly = await _cosmosDbProvider.DoesCustomerHaveATerminationDate(customerGuid);

            if (isCustomerReadOnly)
                return new ObjectResult(customerGuid)
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };

            var doesInteractionExist = await _cosmosDbProvider.DoesInteractionResourceExistAndBelongToCustomerAsync(interactionGuid, customerGuid);

            if (!doesInteractionExist)
                return new NoContentResult();

            var webChat = await _webChatPatchService.GetWebChatForCustomerAsync(customerGuid, interactionGuid, webChatGuid);

            if (webChat == null)
                return new NoContentResult();

            var updatedWebChat = await _webChatPatchService.UpdateAsync(webChat, webChatPatchRequest);

            if (updatedWebChat != null)
                await _webChatPatchService.SendToServiceBusQueueAsync(updatedWebChat, customerGuid, ApimURL);

            return updatedWebChat == null ?
                new BadRequestObjectResult(webChatGuid) :
                new JsonResult(updatedWebChat, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
        }
    }
}