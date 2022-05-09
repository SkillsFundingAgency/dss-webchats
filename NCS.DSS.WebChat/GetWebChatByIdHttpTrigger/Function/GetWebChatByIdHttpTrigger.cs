using System;
using System.ComponentModel.DataAnnotations;
using DFC.Swagger.Standard.Annotations;
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
using NCS.DSS.WebChat.Cosmos.Helper;
using NCS.DSS.WebChat.GetWebChatByIdHttpTrigger.Service;

namespace NCS.DSS.WebChat.GetWebChatByIdHttpTrigger.Function
{
    public class GetWebChatByIdHttpTrigger
    {
        private IResourceHelper _resourceHelper;
        private IHttpRequestHelper _httpRequestMessageHelper;
        private IGetWebChatByIdHttpTriggerService _webChatGetService;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;

        public GetWebChatByIdHttpTrigger(IResourceHelper resourceHelper,
            IHttpRequestHelper httpRequestMessageHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IJsonHelper jsonHelper,
            IGetWebChatByIdHttpTriggerService webChatGetService)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _webChatGetService = webChatGetService;
            _jsonHelper = jsonHelper;
        }

        [FunctionName("GetById")]
        [ProducesResponseType(typeof(Models.WebChat), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "WebChat found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "WebChat does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual webchat record.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/WebChats/{webChatId}")]HttpRequest req, ILogger log, string customerId, string interactionId, string webChatId)
        {
            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return _httpResponseMessageHelper.BadRequest();
            }

            var subcontractorId = _httpRequestMessageHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                log.LogInformation("Unable to locate 'SubcontractorId' in request header.");
                return _httpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Get Web Chat By Id C# HTTP trigger function  processed a request. By Touchpoint. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return _httpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return _httpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(webChatId, out var webChatGuid))
                return _httpResponseMessageHelper.BadRequest(webChatGuid);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return _httpResponseMessageHelper.NoContent(customerGuid);

            var doesInteractionExist = _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
                return _httpResponseMessageHelper.NoContent(interactionGuid);

            var webChat = await _webChatGetService.GetWebChatForCustomerAsync(customerGuid, interactionGuid, webChatGuid);

            return webChat == null ?
                _httpResponseMessageHelper.NoContent(webChatGuid) :
                _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(webChat,"id", "WebChatId"));
        }
    }
}