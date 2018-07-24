using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.WebChat.Models;

namespace NCS.DSS.WebChat.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        bool DoesCustomerResourceExist(Guid customerId);
        bool DoesInteractionResourceExist(Guid interactionId);
        Task<List<Models.WebChat>> GetWebChatsForCustomerAsync(Guid customerId);
        Task<Models.WebChat> GetWebChatForCustomerAsync(Guid customerId, Guid webchatId);
        Task<ResourceResponse<Document>> CreateWebChatAsync(Models.WebChat webchat);
        Task<ResourceResponse<Document>> UpdateWebChatAsync(Models.WebChat webchat);
    }
}