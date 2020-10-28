using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.WebChat.Annotations;
using NCS.DSS.WebChat.Cosmos.Helper;
using NCS.DSS.WebChat.GetWebChatByIdHttpTrigger.Service;
using NCS.DSS.WebChat.Helpers;

namespace NCS.DSS.WebChat.GetWebChatByIdHttpTrigger.Function
{
    public class GetWebChatByIdHttpTrigger
    {
        private IResourceHelper _resourceHelper;
        private IHttpRequestMessageHelper _httpRequestMessageHelper;
        private IGetWebChatByIdHttpTriggerService _webChatGetService;

        public GetWebChatByIdHttpTrigger(IResourceHelper resourceHelper,
            IHttpRequestMessageHelper httpRequestMessageHelper,
            IGetWebChatByIdHttpTriggerService webChatGetService)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _webChatGetService = webChatGetService;
        }

        [FunctionName("GetById")]
        [ResponseType(typeof(Models.WebChat))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "WebChat found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "WebChat does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual webchat record.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/WebChats/{webChatId}")]HttpRequestMessage req, ILogger log, string customerId, string interactionId, string webChatId)
        {
            var touchpointId = _httpRequestMessageHelper.GetTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return HttpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Get Web Chat By Id C# HTTP trigger function  processed a request. By Touchpoint. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(webChatId, out var webChatGuid))
                return HttpResponseMessageHelper.BadRequest(webChatGuid);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var doesInteractionExist = _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var webChat = await _webChatGetService.GetWebChatForCustomerAsync(customerGuid, interactionGuid, webChatGuid);

            return webChat == null ?
                HttpResponseMessageHelper.NoContent(webChatGuid) :
                HttpResponseMessageHelper.Ok(JsonHelper.SerializeObject(webChat));
        }
    }
}