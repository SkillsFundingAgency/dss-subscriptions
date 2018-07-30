using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Subscriptions.Cosmos.Client
{
    public interface IDocumentDBClient
    {
        DocumentClient CreateDocumentClient();
    }
}