using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace NCS.DSS.WebChat.PatchWebChatHttpTrigger
{
    public static class PatchWebChatHttpTrigger
    {
        [FunctionName("Patch")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId:guid}/Interactions/{interactionId:guid}/WebChats/{webChatId:guid}")]HttpRequestMessage req, TraceWriter log, string webChatId)
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