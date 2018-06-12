using System;

namespace NCS.DSS.WebChat.PostWebChatHttpTrigger
{
    public class PostWebChatHttpTriggerService
    {
        public Guid? Create(Models.WebChat webChat)
        {
            if (webChat == null)
                return null;

            return Guid.NewGuid();
        }
    }
}