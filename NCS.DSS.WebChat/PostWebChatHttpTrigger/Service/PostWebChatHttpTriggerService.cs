using Microsoft.Extensions.Logging;
using NCS.DSS.WebChat.Cosmos.Provider;
using NCS.DSS.WebChat.ServiceBus;
using System.Net;
namespace NCS.DSS.WebChat.PostWebChatHttpTrigger.Service
{
    public class PostWebChatHttpTriggerService : IPostWebChatHttpTriggerService
    {
        private readonly IWebChatServiceBusClient _serviceBusClient;
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly ILogger<PostWebChatHttpTriggerService> _logger;

        public PostWebChatHttpTriggerService(ICosmosDBProvider cosmosDbProvider, IWebChatServiceBusClient serviceBusClient,ILogger<PostWebChatHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _serviceBusClient = serviceBusClient;   
            _logger = logger;
        }
        public async Task<Models.WebChat> CreateAsync(Models.WebChat webChat)
        {
            if (webChat == null)
            {
                _logger.LogWarning("WebChat Object not found");
                return null; 
            }
            _logger.LogInformation("Attempting to Set Default values for WebChat with ID {WebChatId}", webChat.WebChatId);
            webChat.SetDefaultValues();

            _logger.LogInformation("Attempting to Create WebChat Record for {WebChatId}", webChat.WebChatId);
            var response = await _cosmosDbProvider.CreateWebChatAsync(webChat);

            if (response.StatusCode == HttpStatusCode.Created) 
            {
                _logger.LogInformation("Successfully Created WebChat Record for {WebChatId}", webChat.WebChatId);
                return response.Resource; 
            }
            _logger.LogInformation("Failed to Create WebChat Record for {WebChatId} and Response Code {StatusCode}", webChat.WebChatId, response.StatusCode);
            return null;
        }

        public async Task SendToServiceBusQueueAsync(Models.WebChat webChat, string reqUrl)
        {
            await _serviceBusClient.SendPostMessageAsync(webChat, reqUrl);
        }
    }
}