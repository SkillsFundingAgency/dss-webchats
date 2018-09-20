﻿using System;
using System.Threading.Tasks;
using NCS.DSS.WebChat.Cosmos.Provider;

namespace NCS.DSS.WebChat.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesCustomerExist = await documentDbProvider.DoesCustomerResourceExist(customerId);

            return doesCustomerExist;
        }

        public async Task<bool> IsCustomerReadOnly(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var isCustomerReadOnly = await documentDbProvider.DoesCustomerHaveATerminationDate(customerId);

            return isCustomerReadOnly;
        }

        public async Task<bool> DoesInteractionExist(Guid interactionId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesInteractionExist = await documentDbProvider.DoesInteractionResourceExist(interactionId);

            return doesInteractionExist;
        }
    }
}
