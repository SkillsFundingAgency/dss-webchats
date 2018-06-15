using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.WebChat.Annotations;
using Newtonsoft.Json;

namespace NCS.DSS.WebChat.DeleteWebChatHttpTrigger
{
    public static class DeleteWebChatHttpTrigger
    {
        [FunctionName("Delete")]
        [WebChatResponse(HttpStatusCode = (int)HttpStatusCode.OK, Description = "WebChat deleted", ShowSchema = true)]
        [WebChatResponse(HttpStatusCode = (int)HttpStatusCode.NotFound, Description = "Supplied WebChat Id does not exist", ShowSchema = false)]
        [Display(Name = "Delete", Description = "Ability to delete an webchat record.")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Customers/{customerId}/Interactions/{interactionId}/WebChats/{webChatId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string webChatId)
        {
            log.Info("Delete Web Chat C# HTTP trigger function processed a request.");

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
                Content = new StringContent("Deleted Web Chat record with Id of : " + webChatGuid)
            };
        }
    }
}