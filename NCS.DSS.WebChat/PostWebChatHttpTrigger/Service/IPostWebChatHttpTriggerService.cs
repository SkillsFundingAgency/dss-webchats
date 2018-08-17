using System.Threading.Tasks;

namespace NCS.DSS.WebChat.PostWebChatHttpTrigger.Service
{
    public interface IPostWebChatHttpTriggerService
    {
        Task<Models.WebChat> CreateAsync(Models.WebChat webChat);
        Task SendToServiceBusQueueAsync(Models.WebChat webChat, string reqUrl);
    }
}