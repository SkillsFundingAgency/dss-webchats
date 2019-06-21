using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.WebChat.Cosmos.Provider;
using NCS.DSS.WebChat.Models;
using NCS.DSS.WebChat.ServiceBus;

namespace NCS.DSS.WebChat.PatchWebChatHttpTrigger.Service
{
    public class PatchWebChatHttpTriggerService : IPatchWebChatHttpTriggerService
    {
        public async Task<Models.WebChat> UpdateAsync(Models.WebChat webChat, WebChatPatch webChatPatch)
        {
            if (webChat == null)
                return null;

            webChat.Patch(webChatPatch);
            webChatPatch.SetDefaultValues();

            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.UpdateWebChatAsync(webChat);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? webChat : null;
        }

        public async Task<Models.WebChat> GetWebChatForCustomerAsync(Guid customerId, Guid interactionId, Guid webChatId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var webChat = await documentDbProvider.GetWebChatForCustomerAsync(customerId, interactionId, webChatId);

            return webChat;
        }

        public async Task SendToServiceBusQueueAsync(Models.WebChat webChat, Guid customerId, string reqUrl)
        {
            await ServiceBusClient.SendPatchMessageAsync(webChat, customerId, reqUrl);
        }
    }
}