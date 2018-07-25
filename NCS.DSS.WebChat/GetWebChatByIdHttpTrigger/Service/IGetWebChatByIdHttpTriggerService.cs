using System;
using System.Threading.Tasks;

namespace NCS.DSS.WebChat.GetWebChatByIdHttpTrigger.Service
{
    public interface IGetWebChatByIdHttpTriggerService
    {
        Task<Models.WebChat> GetWebChatForCustomerAsync(Guid customerGuid, Guid interactionGuid, Guid webChatGuid);
    }
}