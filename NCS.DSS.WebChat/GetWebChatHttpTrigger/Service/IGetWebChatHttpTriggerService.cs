using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.WebChat.GetWebChatHttpTrigger.Service
{
    public interface IGetWebChatHttpTriggerService
    {
        Task<List<Models.WebChat>> GetWebChatsForCustomerAsync(Guid customerId, Guid interactionId);
    }
}