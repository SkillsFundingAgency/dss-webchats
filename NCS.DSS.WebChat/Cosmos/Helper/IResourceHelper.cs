using System;

namespace NCS.DSS.WebChat.Cosmos.Helper
{
    public interface IResourceHelper
    {
        bool DoesCustomerExist(Guid customerId);
        bool DoesInteractionExist(Guid interactionId);
    }
}