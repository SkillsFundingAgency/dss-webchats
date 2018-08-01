﻿using System;
using System.Threading.Tasks;

namespace NCS.DSS.WebChat.PatchWebChatHttpTrigger.Service
{
    public interface IPatchWebChatHttpTriggerService
    {
        Task<Models.WebChat> UpdateAsync(Models.WebChat webChat, Models.WebChatPatch webChatPatch);
        Task<Models.WebChat> GetWebChatForCustomerAsync(Guid customerGuid, Guid interactionGuid, Guid webChatGuid);
    }
}