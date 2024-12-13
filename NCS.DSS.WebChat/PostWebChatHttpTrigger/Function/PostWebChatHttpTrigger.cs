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
                //return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            _logger.LogInformation("Post Web Chat C# HTTP trigger function processed a request. By Touchpoint. " + touchpointId);

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
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, ["TargetSite"]));
            }

            if (webChatRequest == null)
                return new UnprocessableEntityObjectResult(req);

            webChatRequest.SetIds(customerGuid, interactionGuid, touchpointId);

            var errors = _validate.ValidateResource(webChatRequest, true);

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