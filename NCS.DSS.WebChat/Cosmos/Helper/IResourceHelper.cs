using System;
using System.Threading.Tasks;

namespace NCS.DSS.WebChat.Cosmos.Helper
{
    public interface IResourceHelper
    {
        Task<bool> DoesCustomerExist(Guid customerId);
        Task<bool> IsCustomerReadOnly(Guid customerId);
        Task<bool> DoesInteractionExist(Guid interactionId);
    }
}