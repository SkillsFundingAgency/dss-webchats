using Microsoft.Extensions.Logging;
using NCS.DSS.WebChat.Cosmos.Provider;
using NCS.DSS.WebChat.Models;
using NCS.DSS.WebChat.ServiceBus;
using System.Net;

namespace NCS.DSS.WebChat.PatchWebChatHttpTrigger.Service
{
    public class PatchWebChatHttpTriggerService : IPatchWebChatHttpTriggerService
    {
        private readonly IWebChatServiceBusClient _serviceBusClient;
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly ILogger<PatchWebChatHttpTriggerService> _logger;

        public PatchWebChatHttpTriggerService(ICosmosDBProvider cosmosDbProvider, 
            IWebChatServiceBusClient serviceBusClient,
            ILogger<PatchWebChatHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _serviceBusClient = serviceBusClient;
            _logger = logger;
        }
        public async Task<Models.WebChat> UpdateAsync(Models.WebChat webChat, WebChatPatch webChatPatch)
        {
            if (webChat == null)
            {
                _logger.LogWarning("WebChat Object not found");
                return null;
            }

            _logger.LogInformation("Preparing PATCH request for WebChat with ID {WebChatId}", webChat.WebChatId);
            webChat.Patch(webChatPatch);
            _logger.LogInformation("Attempting to Set Default values for WebChat with ID {WebChatId}", webChat.WebChatId);
            webChat.SetDefaultValues();

            _logger.LogInformation("Attempting to Update WebChat Record for {WebChatId}", webChat.WebChatId);
            var response = await _cosmosDbProvider.UpdateWebChatAsync(webChat);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("Successfully Updated WebChat Record for {WebChatId}", webChat.WebChatId);
                return response.Resource;
            }
            _logger.LogInformation("Failed to Updated WebChat Record for {WebChatId} and Response Code {StatusCode}", webChat.WebChatId, response.StatusCode);
            return null;                       
        }

        public async Task<Models.WebChat> GetWebChatForCustomerAsync(Guid customerId, Guid interactionId, Guid webChatId)
        {
            return await _cosmosDbProvider.GetWebChatForCustomerAsync(customerId, interactionId, webChatId); ;
        }

        public async Task SendToServiceBusQueueAsync(Models.WebChat webChat, Guid customerId, string reqUrl)
        {
            await _serviceBusClient.SendPatchMessageAsync(webChat, customerId, reqUrl);
        }
    }
}