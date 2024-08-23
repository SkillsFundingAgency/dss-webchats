using NCS.DSS.WebChat.Cosmos.Provider;

namespace NCS.DSS.WebChat.GetWebChatHttpTrigger.Service
{
    public class GetWebChatHttpTriggerService : IGetWebChatHttpTriggerService
    {
        public async Task<List<Models.WebChat>> GetWebChatsForCustomerAsync(Guid customerId, Guid interactionId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var webChats = await documentDbProvider.GetWebChatsForCustomerAsync(customerId, interactionId);

            return webChats;
        }
    }
}