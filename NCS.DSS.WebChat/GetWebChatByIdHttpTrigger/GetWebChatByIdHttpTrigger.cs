using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Description;
using NCS.DSS.WebChat.Annotations;

namespace NCS.DSS.WebChat.GetWebChatByIdHttpTrigger
{
    public static class GetWebChatByIdHttpTrigger
    {
        [FunctionName("GetById")]
        [ResponseType(typeof(Models.WebChat))]
        [WebChatResponse(HttpStatusCode = (int)HttpStatusCode.OK, Description = "WebChat found", ShowSchema = true)]
        [WebChatResponse(HttpStatusCode = (int)HttpStatusCode.NotFound, Description = "Supplied WebChat Id does not exist", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual webchat record.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/WebChats/{webChatId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string webChatId)
        {
            log.Info("Get Web Chat By Id C# HTTP trigger function  processed a request.");

            if (!Guid.TryParse(webChatId, out var webChatGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(webChatId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            var webChatService = new GetWebChatByIdHttpTriggerService();
            var webChat = await webChatService.GetWebChat(webChatGuid);

            if (webChat == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(
                        "Unable to find Web Chat record with Id of : " + webChatGuid)
                };

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(webChat),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}