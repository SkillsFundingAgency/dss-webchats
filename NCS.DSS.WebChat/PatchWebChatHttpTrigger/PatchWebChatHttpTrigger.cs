using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.WebChat.Annotations;
using Newtonsoft.Json;

namespace NCS.DSS.WebChat.PatchWebChatHttpTrigger
{
    public static class PatchWebChatHttpTrigger
    {
        [FunctionName("Patch")]
        [ResponseType(typeof(Models.WebChat))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "WebChat Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "WebChat does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "WebChat validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update an webchat record.")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/WebChats/{webChatId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string webChatId)
        {
            log.Info("Patch Web Chat C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(webChatId, out var webChatGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(webChatId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Updated Web Chat record with Id of : " + webChatGuid)
            };
        }
    }
}