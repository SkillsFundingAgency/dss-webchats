namespace NCS.DSS.WebChat.ServiceBus
{
    public interface IWebChatServiceBusClient
    {
        Task SendPostMessageAsync(Models.WebChat webChat, string reqUrl);
        Task SendPatchMessageAsync(Models.WebChat webChat, Guid customerId, string reqUrl);
    } 
}

