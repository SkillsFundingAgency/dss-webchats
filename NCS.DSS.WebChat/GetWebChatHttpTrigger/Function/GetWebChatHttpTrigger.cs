using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.WebChat.Cosmos.Provider;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.WebChat.GetWebChatHttpTrigger.Function
{
    public class GetWebChatHttpTrigger
    {
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly ILogger<GetWebChatHttpTrigger> _logger;

        public GetWebChatHttpTrigger(ICosmosDBProvider cosmosDbProvider,
            IHttpRequestHelper httpRequestMessageHelper,
            ILogger<GetWebChatHttpTrigger> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _logger = logger;
        }

        [Function("Get")]
        [ProducesResponseType(typeof(Models.WebChat), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "WebChats found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "WebChats do not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to return all webchat records for a given customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/WebChats")] HttpRequest req, string customerId, string interactionId)

        {
            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            _logger.LogInformation("Get Web Chat C# HTTP trigger function processed a request. By Touchpoint. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return new BadRequestObjectResult(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return new BadRequestObjectResult(interactionGuid);

            var doesCustomerExist = await _cosmosDbProvider.DoesCustomerResourceExist(customerGuid);

            if (!doesCustomerExist)
                return new NoContentResult();

            var doesInteractionExist = await _cosmosDbProvider.DoesInteractionResourceExistAndBelongToCustomerAsync(interactionGuid, customerGuid);

            if (!doesInteractionExist)
                return new NoContentResult();

            var webChats = await _cosmosDbProvider.GetWebChatsForCustomerAsync(customerGuid, interactionGuid);

            return webChats == null ?
                new NoContentResult() :
                webChats.Count == 1 ? new JsonResult(webChats[0], new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                } :
                    new JsonResult(webChats, new JsonSerializerOptions())
                    {
                        StatusCode = (int)HttpStatusCode.OK
                    };
        }
    }
}