using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.WebChat.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        bool DoesCustomerResourceExist(Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        bool DoesInteractionResourceExist(Guid interactionId);
        Task<List<Models.WebChat>> GetWebChatsForCustomerAsync(Guid customerId, Guid interactionId);
        Task<Models.WebChat> GetWebChatForCustomerAsync(Guid customerId, Guid interactionId, Guid webchatId);
        Task<ResourceResponse<Document>> CreateWebChatAsync(Models.WebChat webchat);
        Task<ResourceResponse<Document>> UpdateWebChatAsync(Models.WebChat webchat);
    }
}