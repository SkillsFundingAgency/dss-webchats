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
        public async Task<IActionResult> RunA([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/WebChats/{webChatId}")] HttpRequest req, string customerId, string interactionId, string webChatId)
        {
            var functionName = nameof(PatchWebChatHttpTrigger);
            _logger.LogInformation("Function {FunctionName} has been invoked", functionName);

            var correlationId = _httpRequestMessageHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");
                correlationId = Guid.NewGuid().ToString();
            }

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");                
            }

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
            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                var response = new BadRequestObjectResult(customerGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to parse 'customerId' to a Guid: {customerId}", correlationId, response.StatusCode, customerId);
                return response;
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                var response = new BadRequestObjectResult(interactionGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to parse 'interactionId' to a Guid: {interactionId}", correlationId, response.StatusCode, interactionId);
                return response;
            }

            if (!Guid.TryParse(webChatId, out var webChatGuid))
            {
                var response = new BadRequestObjectResult(webChatGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to parse 'WebChatId' to a Guid: {WebChatId}", correlationId, response.StatusCode, webChatGuid);
                return response;
            }

            _logger.LogInformation("{CorrelationId} Input validation has succeeded.", correlationId);
            WebChatPatch webChatPatchRequest;

            try
            {
                _logger.LogInformation("{CorrelationId} Attempt to get resource from body of the request", correlationId);
                webChatPatchRequest = await _httpRequestMessageHelper.GetResourceFromRequest<Models.WebChatPatch>(req);
            }
            catch (Exception ex)
            {
                var response = new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, ["TargetSite"]));
                _logger.LogError(ex, "{CorrelationId} Response Status Code: {StatusCode}. Unable to retrieve body from req", correlationId, response.StatusCode);
                return response;
            }

            if (webChatPatchRequest == null)
            {
                var response = new UnprocessableEntityObjectResult(req);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. WebChat patch request is null", correlationId, response.StatusCode);
                return response;
            }

            _logger.LogInformation("{CorrelationId} Attempt to set Default Values for WebChat patch", correlationId);
            webChatPatchRequest.LastModifiedTouchpointId = touchpointId;
            webChatPatchRequest.SetDefaultValues();

            _logger.LogInformation("{CorrelationId} Attempt to validate resource", correlationId);
            var errors = _validate.ValidateResource(webChatPatchRequest, false);

            if (errors != null && errors.Any())
            {
                var response = new UnprocessableEntityObjectResult(errors);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. validation errors with resource {Errors}", correlationId, response.StatusCode, string.Join(',', errors));
                return response;
            }
            _logger.LogInformation("{CorrelationId} Attempting to see if customer exists {CustomerId}", correlationId, customerGuid);
            var doesCustomerExist = await _cosmosDbProvider.DoesCustomerResourceExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response = new NoContentResult();
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Customer does not exist {CustomerId}", correlationId, customerGuid);
                return response;
            }
            _logger.LogInformation("{CorrelationId} Customer record found in Cosmos DB {customerGuid}", correlationId, customerGuid);

            _logger.LogInformation("{CorrelationId} Attempting to see if this is a read only customer {CustomerId}", correlationId, customerGuid);
            var isCustomerReadOnly = await _cosmosDbProvider.DoesCustomerHaveATerminationDate(customerGuid);

            if (isCustomerReadOnly)
            {
                var response = new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Customer is read only {CustomerId}", correlationId, response.StatusCode, customerGuid);
                return response;
            }

            _logger.LogInformation("{CorrelationId} Attempting to see if interaction exists {InteractionId}", correlationId, interactionGuid);
            var doesInteractionExist = await _cosmosDbProvider.DoesInteractionResourceExistAndBelongToCustomerAsync(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                var response = new NoContentResult();
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Interaction does not exist {InteractionId}", correlationId, response.StatusCode, interactionGuid);
                return response;
            }
            _logger.LogInformation("{CorrelationId} Interaction record with {interactionGuid} found in Cosmos DB for Customer {customerGuid}", correlationId, interactionGuid, customerGuid);

            _logger.LogInformation("{CorrelationId} Attempting to get webchat for customer {CustomerId}", correlationId, customerGuid);
            var webChat = await _webChatPatchService.GetWebChatForCustomerAsync(customerGuid, interactionGuid, webChatGuid);

            if (webChat == null)
            {
                var response = new NoContentResult();
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. WebChat does not exist {WebChatId}", correlationId, response.StatusCode, webChatGuid);
                return response;
            }
            _logger.LogInformation("{CorrelationId} Attempting to update WebChat with an ID {WebChatId}", correlationId, webChatGuid);
            var updatedWebChat = await _webChatPatchService.UpdateAsync(webChat, webChatPatchRequest);

            if (updatedWebChat == null)
            {
                var response = new BadRequestObjectResult(webChatGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Failed to patch the session {SessionId}", correlationId, response.StatusCode, webChatGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return response;
            }
            else
            {
                _logger.LogInformation("{CorrelationId} Attempting to send to service bus {SessionId}", correlationId, webChatGuid);
                await _webChatPatchService.SendToServiceBusQueueAsync(updatedWebChat, customerGuid, ApimURL);
                var response = new JsonResult(updatedWebChat, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
                _logger.LogInformation("{CorrelationId} Response Status Code: {StatusCode}. Successfully patched the session {SessionId}", correlationId, response.StatusCode, webChatGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return response;
            }
        }
    }
}