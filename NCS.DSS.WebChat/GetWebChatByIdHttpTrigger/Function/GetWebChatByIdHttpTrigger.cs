using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.WebChat.Cosmos.Helper;
using NCS.DSS.WebChat.GetWebChatByIdHttpTrigger.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.WebChat.GetWebChatByIdHttpTrigger.Function
{
    public class GetWebChatByIdHttpTrigger
    {
        private IResourceHelper _resourceHelper;
        private IHttpRequestHelper _httpRequestMessageHelper;
        private IGetWebChatByIdHttpTriggerService _webChatGetService;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private ILogger log;

        public GetWebChatByIdHttpTrigger(IResourceHelper resourceHelper,
            IHttpRequestHelper httpRequestMessageHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IJsonHelper jsonHelper,
            IGetWebChatByIdHttpTriggerService webChatGetService,
            ILogger<GetWebChatByIdHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _webChatGetService = webChatGetService;
            _jsonHelper = jsonHelper;
            log = logger;
        }

        [Function("GetById")]
        [ProducesResponseType(typeof(Models.WebChat), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "WebChat found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "WebChat does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual webchat record.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/WebChats/{webChatId}")] HttpRequest req, string customerId, string interactionId, string webChatId)
        {
            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            log.LogInformation("Get Web Chat By Id C# HTTP trigger function  processed a request. By Touchpoint. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return new BadRequestObjectResult(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return new BadRequestObjectResult(interactionGuid);

            if (!Guid.TryParse(webChatId, out var webChatGuid))
                return new BadRequestObjectResult(webChatGuid);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return new NoContentResult();

            var doesInteractionExist = _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
                return new NoContentResult();

            var webChat = await _webChatGetService.GetWebChatForCustomerAsync(customerGuid, interactionGuid, webChatGuid);

            return webChat == null ?
                new NoContentResult() :
                new JsonResult(webChat, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
        }
    }
}