using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.WebChat.Cosmos.Client;
using NCS.DSS.WebChat.Cosmos.Helper;

namespace NCS.DSS.WebChat.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        private readonly DocumentDBHelper _documentDbHelper;
        private readonly DocumentDBClient _databaseClient;

        public DocumentDBProvider()
        {
            _documentDbHelper = new DocumentDBHelper();
            _databaseClient = new DocumentDBClient();
        }

        public bool DoesCustomerResourceExist(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateCustomerDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var customerQuery = client.CreateDocumentQuery<Document>(collectionUri, new FeedOptions() { MaxItemCount = 1 });
            return customerQuery.Where(x => x.Id == customerId.ToString()).Select(x => x.Id).AsEnumerable().Any();
        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateCustomerDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            var customerByIdQuery = client
                ?.CreateDocumentQuery<Document>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.Id == customerId.ToString())
                .AsDocumentQuery();

            if (customerByIdQuery == null)
                return false;

            var customerQuery = await customerByIdQuery.ExecuteNextAsync<Document>();

            var customer = customerQuery?.FirstOrDefault();

            if (customer == null)
                return false;

            var dateOfTermination = customer.GetPropertyValue<DateTime?>("DateOfTermination");

            return dateOfTermination.HasValue;
        }

        public bool DoesInteractionResourceExist(Guid interactionId)
        {
            var collectionUri = _documentDbHelper.CreateInteractionDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var interactionQuery = client.CreateDocumentQuery<Document>(collectionUri, new FeedOptions() { MaxItemCount = 1 });
            return interactionQuery.Where(x => x.Id == interactionId.ToString()).Select(x => x.Id).AsEnumerable().Any();
        }

        public async Task<List<Models.WebChat>> GetWebChatsForCustomerAsync(Guid customerId, Guid interactionId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var webchatsQuery = client.CreateDocumentQuery<Models.WebChat>(collectionUri)
                .Where(so => so.CustomerId == customerId &&
                             so.InteractionId == interactionId).AsDocumentQuery();

            var webchats = new List<Models.WebChat>();

            while (webchatsQuery.HasMoreResults)
            {
                var response = await webchatsQuery.ExecuteNextAsync<Models.WebChat>();
                webchats.AddRange(response);
            }

            return webchats.Any() ? webchats : null;
        }

        public async Task<Models.WebChat> GetWebChatForCustomerAsync(Guid customerId, Guid interactionId, Guid webchatId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            var webchatForCustomerQuery = client
                ?.CreateDocumentQuery<Models.WebChat>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId &&
                            x.InteractionId == interactionId &&
                            x.WebChatId == webchatId)
                .AsDocumentQuery();

            if (webchatForCustomerQuery == null)
                return null;

            var webchats = await webchatForCustomerQuery.ExecuteNextAsync<Models.WebChat>();

            return webchats?.FirstOrDefault();
        }

        public async Task<ResourceResponse<Document>> CreateWebChatAsync(Models.WebChat webchat)
        {

            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, webchat);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateWebChatAsync(Models.WebChat webchat)
        {
            var documentUri = _documentDbHelper.CreateDocumentUri(webchat.WebChatId.GetValueOrDefault());

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, webchat);

            return response;
        }
    }
}