using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.WebChat.Cosmos.Provider;
using NCS.DSS.WebChat.Helpers;
using NCS.DSS.WebChat.PostWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace NCS.DSS.WebChat.PostWebChatHttpTrigger.Function
{
    public class PostWebChatHttpTrigger
    {
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly IValidate _validate;
        private readonly IPostWebChatHttpTriggerService _webChatPostService;
        private readonly ILogger<PostWebChatHttpTrigger> _logger;
        private readonly IDynamicHelper _dynamicHelper;

        public PostWebChatHttpTrigger(ICosmosDBProvider cosmosDbProvider,
            IHttpRequestHelper httpRequestMessageHelper,
            IValidate validate,
            IPostWebChatHttpTriggerService webChatPostService,
            ILogger<PostWebChatHttpTrigger> logger,
            IDynamicHelper dynamicHelper)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _webChatPostService = webChatPostService;
            _validate = validate;
            _logger = logger;
            _dynamicHelper = dynamicHelper;
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

            var functionName = nameof(PostWebChatHttpTrigger);
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
                _logger.LogWarning("Unable to locate 'TouchpointId' in request header.");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            var ApimURL = _httpRequestMessageHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                _logger.LogWarning("Unable to locate 'apimurl' in request header");
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
            _logger.LogInformation("{CorrelationId} Input validation has succeeded.", correlationId);


            Models.WebChat webChatRequest;

            try
            {
                webChatRequest = await _httpRequestMessageHelper.GetResourceFromRequest<Models.WebChat>(req);
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, ["TargetSite", "StackTrace"]));
            }

            if (webChatRequest == null)
            {
                var response = new UnprocessableEntityObjectResult(req);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. WebChat patch request is null", correlationId, response.StatusCode);
                return response;
            }
            _logger.LogInformation("{CorrelationId} Attempt to set id's for Web Chat patch", correlationId);
            webChatRequest.SetIds(customerGuid, interactionGuid, touchpointId);

            _logger.LogInformation("{CorrelationId} Attempt to validate resource", correlationId);
            var errors = _validate.ValidateResource(webChatRequest, true);

            if (errors != null && errors.Count != 0)
            {
                var response = new UnprocessableEntityObjectResult(errors);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. validation errors with resource {Errors}", correlationId, response.StatusCode,string.Join(',', errors));
                return response;
            }

            var doesCustomerExist = await _cosmosDbProvider.DoesCustomerResourceExist(customerGuid);

            _logger.LogInformation("{CorrelationId} Attempting to see if customer exists {CustomerId}", correlationId, customerGuid);
            if (!doesCustomerExist)
            {
                var response = new NoContentResult();
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Customer does not exist {CustomerId}", correlationId, customerGuid);
                return response;
            }

            _logger.LogInformation("{CorrelationId} Customer record found in Cosmos DB {customerGuid}", correlationId, customerGuid);
            

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

            _logger.LogInformation("{CorrelationId} Attempting to Create Web Chat for customer {customerGuid}", correlationId, customerGuid);
            var webChat = await _webChatPostService.CreateAsync(webChatRequest);

            if (webChat == null)
            {
                var response = new BadRequestObjectResult(customerGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Failed to post a WebChat for customer {customerGuid}", correlationId, response.StatusCode);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return response;
            }
            else
            {
                _logger.LogInformation("{CorrelationId} Attempting to send to service bus {WebChatId}", correlationId, webChat.WebChatId);
                await _webChatPostService.SendToServiceBusQueueAsync(webChat, ApimURL);
                var response = new JsonResult(webChat, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.Created
                };
                _logger.LogInformation("{CorrelationId} Response Status Code: {StatusCode}. Successfully posted a WebChat {WebChatId} for customer {customerGuid}", correlationId, response.StatusCode, webChat.WebChatId, customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return response;
            }
        }
    }
}