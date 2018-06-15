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

namespace NCS.DSS.WebChat.GetWebChatHttpTrigger
{
    public static class GetWebChatHttpTrigger
    {
        [FunctionName("Get")]
        [ResponseType(typeof(Models.WebChat))]
        [WebChatResponse(HttpStatusCode = (int)HttpStatusCode.OK, Description = "WebChats found", ShowSchema = true)]
        [Display(Name = "Get", Description = "Ability to return all webchat records for a given customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/WebChats")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId)
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