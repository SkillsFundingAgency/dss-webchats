﻿namespace NCS.DSS.WebChat.Cosmos.Helper
{
    public interface IResourceHelper
    {
        Task<bool> DoesCustomerExist(Guid customerId);
        Task<bool> IsCustomerReadOnly(Guid customerId);
        bool DoesInteractionResourceExistAndBelongToCustomer(Guid interactionId, Guid customerId);
    }
}