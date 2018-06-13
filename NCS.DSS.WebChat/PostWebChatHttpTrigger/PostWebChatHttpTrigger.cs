using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace NCS.DSS.WebChat.PostWebChatHttpTrigger
{
    public static class PostWebChatHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.WebChat))]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId:guid}/Interactions/{interactionId:guid}/WebChats")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Post Web Chat C# HTTP trigger function processed a request.");

            // Get request body
            var webChat = await req.Content.ReadAsAsync<Models.WebChat>();

            var webChatService = new PostWebChatHttpTriggerService();
            var webChatId = webChatService.Create(webChat);

            return webChatId == null
                ? new HttpResponseMessage(HttpStatusCode.BadRequest)
                : new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent("Created Web Chat record with Id of : " + webChatId)
                };
        }
    }
}