using System;

namespace NCS.DSS.WebChat.PostWebChatHttpTrigger.Service
{
    public class PostWebChatHttpTriggerService : IPostWebChatHttpTriggerService
    {
        public Guid? Create(Models.WebChat webChat)
        {
            if (webChat == null)
                return null;

            return Guid.NewGuid();
        }
    }
}