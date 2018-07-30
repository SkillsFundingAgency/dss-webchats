using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.WebChat.Cosmos.Provider;

namespace NCS.DSS.WebChat.PostWebChatHttpTrigger.Service
{
    public class PostWebChatHttpTriggerService : IPostWebChatHttpTriggerService
    {
        public async Task<Models.WebChat> CreateAsync(Models.WebChat webChat)
        {
            if (webChat == null)
                return null;

           webChat.SetDefaultValues();

            var documentDbProvider = new DocumentDBProvider();

            var response = await documentDbProvider.CreateWebChatAsync(webChat);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }
    }
}