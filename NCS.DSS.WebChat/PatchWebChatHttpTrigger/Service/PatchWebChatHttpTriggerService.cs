using NCS.DSS.WebChat.Cosmos.Provider;
using NCS.DSS.WebChat.Models;
using NCS.DSS.WebChat.ServiceBus;
using System.Net;

namespace NCS.DSS.WebChat.PatchWebChatHttpTrigger.Service
{
    public class PatchWebChatHttpTriggerService : IPatchWebChatHttpTriggerService
    {
        private readonly IWebChatServiceBusClient _serviceBusClient;
        private readonly ICosmosDBProvider _cosmosDbProvider;

        public PatchWebChatHttpTriggerService(ICosmosDBProvider cosmosDbProvider, IWebChatServiceBusClient serviceBusClient)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _serviceBusClient = serviceBusClient;
        }
        public async Task<Models.WebChat> UpdateAsync(Models.WebChat webChat, WebChatPatch webChatPatch)
        {
            if (webChat == null)
                return null;

            webChat.Patch(webChatPatch);
            webChat.SetDefaultValues();

            var response = await _cosmosDbProvider.UpdateWebChatAsync(webChat);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? webChat : null;
        }

        public async Task<Models.WebChat> GetWebChatForCustomerAsync(Guid customerId, Guid interactionId, Guid webChatId)
        {

            var webChat = await _cosmosDbProvider.GetWebChatForCustomerAsync(customerId, interactionId, webChatId);

            return webChat;
        }

        public async Task SendToServiceBusQueueAsync(Models.WebChat webChat, Guid customerId, string reqUrl)
        {
            await _serviceBusClient.SendPatchMessageAsync(webChat, customerId, reqUrl);
        }
    }
}