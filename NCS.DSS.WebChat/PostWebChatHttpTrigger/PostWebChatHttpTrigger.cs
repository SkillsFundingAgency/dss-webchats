using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.WebChat.Annotations;

namespace NCS.DSS.WebChat.PostWebChatHttpTrigger
{
    public static class PostWebChatHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.WebChat))]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "WebChat Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "WebChat does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "WebChat validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new webchat resource.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/{interactionId}/WebChats")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId)
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