using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NCS.DSS.WebChat.PutWebChatHttpTrigger
{
    public static class PutWebChatHttpTrigger
    {
        /*[Disable]
        [Function("Put")]
        [Display(Name = "Put", Description = "Ability to replace an entire webchat record.")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "Customers/{customerId}/Interactions/{interactionId}/WebChats/{webChatId}")]HttpRequest req, ILogger log, string customerId, string interactionId, string webChatId)
        {
            log.LogInformation("Put Web Chat C# HTTP trigger function processed a request.");

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
        }*/
    }
}