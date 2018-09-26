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

        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            var documentUri = DocumentDBHelper.CreateCustomerDocumentUri(customerId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;
            try
            {
                var response = await client.ReadDocumentAsync(documentUri);
                if (response.Resource != null)
                    return true;
            }
            catch (DocumentClientException)
            {
                return false;
            }

            return false;
        }

        public bool DoesInteractionResourceExistAndBelongToCustomer(Guid interactionId, Guid customerId)
        {
            var collectionUri = DocumentDBHelper.CreateInteractionDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var query = client.CreateDocumentQuery<long>(collectionUri, new SqlQuerySpec()
                {
                    QueryText = "SELECT VALUE COUNT(1) FROM interactions i " +
                                "WHERE i.id = @interactionId " +
                                "AND i.CustomerId = @customerId",

                    Parameters = new SqlParameterCollection()
                    {
                        new SqlParameter("@interactionId", interactionId),
                        new SqlParameter("@customerId", customerId)
                    }
                }).AsEnumerable().FirstOrDefault();

                return Convert.ToBoolean(Convert.ToInt16(query));
            }
            catch (DocumentQueryException)
            {
                return false;
            }

        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            var documentUri = DocumentDBHelper.CreateCustomerDocumentUri(customerId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var response = await client.ReadDocumentAsync(documentUri);

                var dateOfTermination = response.Resource?.GetPropertyValue<DateTime?>("DateOfTermination");

                return dateOfTermination.HasValue;
            }
            catch (DocumentClientException)
            {
                return false;
            }
        }

        public async Task<List<Models.WebChat>> GetWebChatsForCustomerAsync(Guid customerId, Guid interactionId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

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
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

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

            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, webchat);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateWebChatAsync(Models.WebChat webchat)
        {
            var documentUri = DocumentDBHelper.CreateDocumentUri(webchat.WebChatId.GetValueOrDefault());

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, webchat);

            return response;
        }
    }
}