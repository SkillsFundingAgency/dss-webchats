using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.WebChat.Cosmos.Provider;
using NCS.DSS.WebChat.Models;

namespace NCS.DSS.WebChat.PatchWebChatHttpTrigger.Service
{
    public class PatchWebChatHttpTriggerService : IPatchWebChatHttpTriggerService
    {
        public async Task<Models.WebChat> UpdateAsync(Models.WebChat webChat, WebChatPatch webChatPatch)
        {
            if (webChat == null)
                return null;

            if (!webChatPatch.LastModifiedDate.HasValue)
                webChatPatch.LastModifiedDate = DateTime.Now;

            webChat.Patch(webChatPatch);

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
    }
}