using NCS.DSS.WebChat.Cosmos.Provider;
using NCS.DSS.WebChat.ServiceBus;
using System.Net;

namespace NCS.DSS.WebChat.PostWebChatHttpTrigger.Service
{
    public class PostWebChatHttpTriggerService : IPostWebChatHttpTriggerService
    {
        private readonly IWebChatServiceBusClient _serviceBusClient;
        private readonly ICosmosDBProvider _cosmosDbProvider;

        public PostWebChatHttpTriggerService(ICosmosDBProvider cosmosDbProvider, IWebChatServiceBusClient serviceBusClient)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _serviceBusClient = serviceBusClient;   
        }
        public async Task<Models.WebChat> CreateAsync(Models.WebChat webChat)
        {
            if (webChat == null)
                return null;

            webChat.SetDefaultValues();

            var response = await _cosmosDbProvider.CreateWebChatAsync(webChat);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.WebChat webChat, string reqUrl)
        {
            await _serviceBusClient.SendPostMessageAsync(webChat, reqUrl);
        }
    }
}