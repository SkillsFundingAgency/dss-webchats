using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using NCS.DSS.WebChat.Models;
using Newtonsoft.Json;
using System.Text;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace NCS.DSS.WebChat.ServiceBus
{
    public class WebChatServiceBusClient : IWebChatServiceBusClient
    {
        private readonly ILogger<WebChatServiceBusClient> _logger;
        public readonly string QueueName = Environment.GetEnvironmentVariable("QueueName");
        private readonly ServiceBusClient _serviceBusClient;
        public WebChatServiceBusClient(ServiceBusClient serviceBusClient, ILogger<WebChatServiceBusClient> logger)
        {
            _serviceBusClient = serviceBusClient;
            _logger = logger;
        }
        public async Task SendPostMessageAsync(Models.WebChat webChat, string reqUrl)
        {
            try
            {
                _logger.LogInformation("Attempting to Create Sender for Service Bus Client");
                var serviceBusSender = _serviceBusClient.CreateSender(QueueName);
                _logger.LogInformation("Preparing Message for Service Bus");
                var messageModel = new MessageModel()
                {
                    TitleMessage = "New WebChat record {" + webChat.InteractionId + "} added at " + DateTime.UtcNow,
                    CustomerGuid = webChat.CustomerId,
                    LastModifiedDate = webChat.LastModifiedDate,
                    URL = reqUrl + "/" + webChat.InteractionId,
                    IsNewCustomer = false,
                    TouchpointId = webChat.LastModifiedTouchpointId
                };

                var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
                {
                    ContentType = "application/json",
                    MessageId = webChat.CustomerId + " " + DateTime.UtcNow
                };
                _logger.LogInformation("Attempting to Send Service Bus Message for WebChat with ID {webchatId}", webChat.WebChatId);
                await serviceBusSender.SendMessageAsync(msg);
                _logger.LogInformation("POST Service Bus Message for WebChat with ID {webchatId} has been sent successfully", webChat.WebChatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Send POST Service Bus Message for WebChat with ID {webchatId}. Exception Raised with {Message}.", webChat.WebChatId, ex.Message);
                throw;
            }
        }

        public async Task SendPatchMessageAsync(Models.WebChat webChat, Guid customerId, string reqUrl)
        {
            try
            {
                _logger.LogInformation("Attempting to Create Sender for Service Bus Client");
                var serviceBusSender = _serviceBusClient.CreateSender(QueueName);
                _logger.LogInformation("Preparing Message for Service Bus");
                var messageModel = new MessageModel
                {
                    TitleMessage = "WebChat record modification for {" + customerId + "} at " + DateTime.UtcNow,
                    CustomerGuid = customerId,
                    LastModifiedDate = webChat.LastModifiedDate,
                    URL = reqUrl,
                    IsNewCustomer = false,
                    TouchpointId = webChat.LastModifiedTouchpointId
                };
                var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
                {
                    ContentType = "application/json",
                    MessageId = customerId + " " + DateTime.UtcNow
                };
                _logger.LogInformation("Attempting to Send Service Bus Message for WebChat with ID {webchatId}", webChat.WebChatId);
                await serviceBusSender.SendMessageAsync(msg);
                _logger.LogInformation("PATCH Service Bus Message for WebChat with ID {webchatId} has been sent successfully", webChat.WebChatId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Send PATCH Service Bus Message for WebChat with ID {webchatId}. Exception Raised with {Message}.", webChat.WebChatId, ex.Message);
                throw;
            }
        }

    } 
}

