using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace NCS.DSS.WebChat.ServiceBus
{
    public static class ServiceBusClient
    {
        public static readonly string KeyName = Environment.GetEnvironmentVariable("KeyName");
        public static readonly string AccessKey = Environment.GetEnvironmentVariable("AccessKey");
        public static readonly string BaseAddress = Environment.GetEnvironmentVariable("BaseAddress");
        public static readonly string QueueName = Environment.GetEnvironmentVariable("QueueName");
        public static readonly string ServiceBusConnectionString = $"Endpoint={BaseAddress};SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey={AccessKey}";
        public static async Task SendPostMessageAsync(Models.WebChat webChat, string reqUrl)
        {
            var queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            var messageModel = new MessageModel()
            {
                TitleMessage = "New WebChat record {" + webChat.InteractionId + "} added at " + DateTime.UtcNow,
                CustomerGuid = webChat.CustomerId,
                LastModifiedDate = webChat.LastModifiedDate,
                URL = reqUrl + "/" + webChat.InteractionId,
                IsNewCustomer = false,
                TouchpointId = webChat.LastModifiedTouchpointId,
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = webChat.CustomerId + " " + DateTime.UtcNow
            };

            await queueClient.SendAsync(msg);
        }

        public static async Task SendPatchMessageAsync(Models.WebChat webChat, Guid customerId, string reqUrl)
        {
            var queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
            var messageModel = new MessageModel
            {
                TitleMessage = "WebChat record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = webChat.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = webChat.LastModifiedTouchpointId,
            };
            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = customerId + " " + DateTime.UtcNow
            };
            await queueClient.SendAsync(msg);
        }

    }

    public class MessageModel
    {
        public string TitleMessage { get; set; }
        public Guid? CustomerGuid { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string URL { get; set; }
        public bool IsNewCustomer { get; set; }
        public string TouchpointId { get; set; }
        public string SubcontractorId { get; set; }
    }

}

