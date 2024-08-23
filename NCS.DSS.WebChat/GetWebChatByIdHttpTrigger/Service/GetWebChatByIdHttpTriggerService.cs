using NCS.DSS.WebChat.Cosmos.Provider;

namespace NCS.DSS.WebChat.GetWebChatByIdHttpTrigger.Service
{
    public class GetWebChatByIdHttpTriggerService : IGetWebChatByIdHttpTriggerService
    {
        public async Task<Models.WebChat> GetWebChatForCustomerAsync(Guid customerId, Guid interactionId, Guid webChatId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var webChat = await documentDbProvider.GetWebChatForCustomerAsync(customerId, interactionId, webChatId);

            return webChat;
        }
    }
}