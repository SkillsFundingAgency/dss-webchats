namespace NCS.DSS.WebChat.PatchWebChatHttpTrigger.Service
{
    public interface IPatchWebChatHttpTriggerService
    {
        Task<Models.WebChat> UpdateAsync(Models.WebChat webChat, Models.WebChatPatch webChatPatch);
        Task<Models.WebChat> GetWebChatForCustomerAsync(Guid customerGuid, Guid interactionGuid, Guid webChatGuid);
        Task SendToServiceBusQueueAsync(Models.WebChat webChat, Guid customerId, string reqUrl);
    }
}