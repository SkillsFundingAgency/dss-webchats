using Microsoft.Azure.Cosmos;

namespace NCS.DSS.WebChat.Cosmos.Provider
{
    public interface ICosmosDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        Task<bool> DoesInteractionResourceExistAndBelongToCustomerAsync(Guid interactionId, Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        Task<List<Models.WebChat>> GetWebChatsForCustomerAsync(Guid customerId, Guid interactionId);
        Task<Models.WebChat> GetWebChatForCustomerAsync(Guid customerId, Guid interactionId, Guid webchatId);
        Task<ItemResponse<Models.WebChat>> CreateWebChatAsync(Models.WebChat webchat);
        Task<ItemResponse<Models.WebChat>> UpdateWebChatAsync(Models.WebChat webchat);
    }
}