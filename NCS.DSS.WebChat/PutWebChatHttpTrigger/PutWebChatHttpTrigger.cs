using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace NCS.DSS.WebChat.PutWebChatHttpTrigger
{
    public static class PutWebChatHttpTrigger
    {
        [Disable]
        [FunctionName("Put")]
        [Display(Name = "Put", Description = "Ability to replace an entire webchat record.")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "Customers/{customerId}/Interactions/{interactionId}/WebChats/{webChatId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string webChatId)
        {
            log.Info("Put Web Chat C# HTTP trigger function processed a request.");

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
                Content = new StringContent("Replaced Web Chat record with Id of : " + webChatGuid)
            };
        }
    }
}