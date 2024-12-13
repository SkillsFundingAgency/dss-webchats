using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using NCS.DSS.WebChat.Models;
using System.Net;
using Microsoft.Extensions.Options;

namespace NCS.DSS.WebChat.Cosmos.Provider
{
    public class CosmosDBProvider : ICosmosDBProvider
    {
        private readonly Container _container;
        private readonly Container _customerContainer;
        private readonly Container _interactionContainer;
        private readonly ILogger<CosmosDBProvider> _logger;
        public CosmosDBProvider(CosmosClient cosmosClient,
            IOptions<WebChatConfigurationSettings> configOptions,
            ILogger<CosmosDBProvider> logger)
        {
            _container = GetContainer(cosmosClient, configOptions.Value.DatabaseId, configOptions.Value.CollectionId);
            _customerContainer = GetContainer(cosmosClient, configOptions.Value.CustomerDatabaseId, configOptions.Value.CustomerCollectionId);
            _interactionContainer = GetContainer(cosmosClient, configOptions.Value.CustomerDatabaseId, configOptions.Value.CustomerCollectionId);
            _logger = logger;
        }
        private static Container GetContainer(CosmosClient cosmosClient, string databaseId, string collectionId)
           => cosmosClient.GetContainer(databaseId, collectionId);

        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            try
            {
                var queryCust = _customerContainer.GetItemLinqQueryable<Customer>().Where(x => x.id == customerId).ToFeedIterator();

                while (queryCust.HasMoreResults)
                {
                    var response = await queryCust.ReadNextAsync();
                    if (response != null && response.Resource.Any())
                    {
                        _logger.LogInformation("Customer Record found in Cosmos DB for {CustomerID}", customerId);
                        return true;
                    }
                }
                _logger.LogWarning("No Customer Record found with {CustomerID} in Cosmos DB", customerId);
                return false;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to find the Customer Record in Cosmos DB {CustomerID}. Exception {Exception}.", customerId, ce.Message);
                throw;
            }
        }

        public async Task<bool> DoesInteractionResourceExistAndBelongToCustomerAsync(Guid interactionId, Guid customerId)
        {
            try
            {
                var queryInt = _interactionContainer.GetItemLinqQueryable<Interaction>()
                                    .Where(x => x.CustomerId == customerId && x.id == interactionId).ToFeedIterator();

                while (queryInt.HasMoreResults)
                {
                    var response = await queryInt.ReadNextAsync();
                    if (response != null && response.Resource.Any())
                    {
                        _logger.LogInformation("Interaction Record found with ID {InteractionId} in Cosmos DB for Customer with ID {CustomerID}", interactionId, customerId);
                        return true;
                    }
                }
                _logger.LogWarning("No Interaction found with ID {InteractionId} and Customer ID {CustomerID} in Cosmos DB", interactionId, customerId);
                return false;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to find the Interaction Record in Cosmos DB {CustomerID}. Exception {Exception}", customerId, ce.Message);
                throw;
            }

        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            try
            {
                var queryCust = _customerContainer.GetItemLinqQueryable<Customer>().Where(x => x.id == customerId).ToFeedIterator();

                while (queryCust.HasMoreResults)
                {
                    var response = await queryCust.ReadNextAsync();
                    var tDate = response.Resource.FirstOrDefault().DateOfTermination;
                    _logger.LogInformation("Customer with {CustomerID} Have a termination date of {tDate} ", customerId, tDate);
                    return tDate.HasValue;
                }
                _logger.LogWarning("No Customer Record found with {CustomerID} in Cosmos DB", customerId);
                return false;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to get DateOfTermination for {CustomerID}. Exception {Exception}.", customerId, ce.Message);
                throw;
            }
        }

        public async Task<List<Models.WebChat>> GetWebChatsForCustomerAsync(Guid customerId, Guid interactionId)
        {
            try
            {
                var queryCdb = _container.GetItemLinqQueryable<Models.WebChat>()
                                    .Where(x => x.CustomerId == customerId && 
                                                x.InteractionId == interactionId).ToFeedIterator();

                while (queryCdb.HasMoreResults)
                {
                    var response = await queryCdb.ReadNextAsync();
                    if (response != null && response.Resource.Any())
                    {
                        _logger.LogInformation("WebChat Records found in Cosmos DB for Customer with ID {CustomerID}", customerId);
                        return response.Resource.ToList();
                    }
                }
                _logger.LogWarning("No WebChat found with {CustomerID} in Cosmos DB", customerId);
                return null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to find the WebChat Record in Cosmos DB {CustomerID}. Exception {Exception}", customerId, ce.Message);
                throw;
            }
        }

        public async Task<Models.WebChat> GetWebChatForCustomerAsync(Guid customerId, Guid interactionId, Guid webchatId)
        {

            try
            {
                var queryCdb = _container.GetItemLinqQueryable<Models.WebChat>()
                                .Where(x => x.CustomerId == customerId && 
                                            x.InteractionId == interactionId &&
                                            x.WebChatId == webchatId).ToFeedIterator();

                while (queryCdb.HasMoreResults)
                {
                    var response = await queryCdb.ReadNextAsync();
                    if (response != null && response.Resource.Any())
                    {
                        _logger.LogInformation("WebChat Record found in Cosmos DB for Customer with ID {CustomerID}", customerId);
                        return response.Resource.FirstOrDefault();
                    }
                }
                _logger.LogWarning("No WebChat Record found with {CustomerID} in Cosmos DB", customerId);
                return null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to find the WebChat Record in Cosmos DB {CustomerID}. Exception {Exception}", customerId, ce.Message);
                throw;
            }
        }

        public async Task<ItemResponse<Models.WebChat>> CreateWebChatAsync(Models.WebChat webchat)
        {
            try
            {
                var response = await _container.CreateItemAsync(webchat, null);
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    _logger.LogInformation("WebChat Record Created in Cosmos DB for {WebChatId}", webchat.WebChatId);
                }
                else
                {
                    _logger.LogError("Failed and returned {StatusCode} to Create WebChat Record in Cosmos DB for {WebChatId}", response.StatusCode, webchat.WebChatId);
                }
                return response;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to Create WebChat Record in Cosmos DB {WebChatId}. Exception {Exception}.", webchat.WebChatId, ce.Message);
                throw;
            }

        }

        public async Task<ItemResponse<Models.WebChat>> UpdateWebChatAsync(Models.WebChat webchat)
        {
            try
            {
                var response = await _container.ReplaceItemAsync(webchat, webchat.WebChatId.ToString());
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation("WebChat Record Updated in Cosmos DB for {WebChatId}", webchat.WebChatId);
                }
                else
                {
                    _logger.LogError("Failed and returned {StatusCode} to Update WebChat Record in Cosmos DB for {WebChatId}", response.StatusCode, webchat.WebChatId);
                }
                return response;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to Update WebChat Record in Cosmos DB {WebChatId}. Exception {Exception}.",webchat.WebChatId, ce.Message);
                throw;
            }
        }
    }
}