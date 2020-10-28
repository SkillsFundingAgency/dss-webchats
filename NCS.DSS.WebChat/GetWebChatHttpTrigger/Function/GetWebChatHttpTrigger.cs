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
using NCS.DSS.WebChat.GetWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.Helpers;

namespace NCS.DSS.WebChat.GetWebChatHttpTrigger.Function
{
    public class GetWebChatHttpTrigger
    {
        private IResourceHelper _resourceHelper;
        private IHttpRequestMessageHelper _httpRequestMessageHelper;
        private IGetWebChatHttpTriggerService _webChatGetService;

        public GetWebChatHttpTrigger(IResourceHelper resourceHelper,
        IHttpRequestMessageHelper httpRequestMessageHelper, IGetWebChatHttpTriggerService webChatGetService)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _webChatGetService = webChatGetService;
        }

        [FunctionName("Get")]
        [ResponseType(typeof(Models.WebChat))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "WebChats found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "WebChats do not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to return all webchat records for a given customer.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/WebChats")]HttpRequestMessage req, ILogger log, string customerId, string interactionId)

        {
            var touchpointId = _httpRequestMessageHelper.GetTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return HttpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Get Web Chat C# HTTP trigger function processed a request. By Touchpoint. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var doesInteractionExist = _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var webChats = await _webChatGetService.GetWebChatsForCustomerAsync(customerGuid, interactionGuid);

            return webChats == null ?
                HttpResponseMessageHelper.NoContent(customerGuid) :
                HttpResponseMessageHelper.Ok(JsonHelper.SerializeObjects(webChats));
        }
    }
}