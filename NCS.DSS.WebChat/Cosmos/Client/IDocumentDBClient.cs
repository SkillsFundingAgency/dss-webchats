using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.WebChat.Cosmos.Client
{
    public interface IDocumentDBClient
    {
        DocumentClient CreateDocumentClient();
    }
}