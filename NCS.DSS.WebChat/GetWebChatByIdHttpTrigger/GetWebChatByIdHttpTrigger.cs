using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.WebChat.GetWebChatByIdHttpTrigger
{
    public static class GetWebChatByIdHttpTrigger
    {
        [FunctionName("GetById")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId:guid}/Interactions/{interactionId:guid}/WebChats/{webChatId:guid}")]HttpRequestMessage req, TraceWriter log, string webChatId)
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