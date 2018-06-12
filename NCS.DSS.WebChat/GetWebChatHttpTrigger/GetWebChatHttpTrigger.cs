using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.WebChat.GetWebChatHttpTrigger
{
    public static class GetWebChatHttpTrigger
    {
        [FunctionName("Get")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId:guid}/Interactions/{interactionId:guid}/WebChats")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Get Web Chat C# HTTP trigger function processed a request.");

            var webChatService = new GetWebChatHttpTriggerService();
            var webChats = await webChatService.GetWebChats();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(webChats),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}