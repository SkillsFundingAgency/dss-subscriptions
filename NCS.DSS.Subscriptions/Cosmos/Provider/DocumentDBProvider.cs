using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.Subscriptions.Cosmos.Client;
using NCS.DSS.Subscriptions.Cosmos.Helper;

namespace NCS.DSS.Subscriptions.Cosmos.Provider
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

        public async Task<Guid?> DoesSubscriptionExist(Guid customerId, string touchpointId, string subcontractorId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            var subscriptionsByIdQuery = client?.CreateDocumentQuery<Models.Subscriptions>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId &&
                            x.TouchPointId == touchpointId &&
                            x.SubcontractorId == subcontractorId &&
                            x.Subscribe == true).AsDocumentQuery(); ;

            if (subscriptionsByIdQuery == null)
                return null;

            var subscription = await subscriptionsByIdQuery.ExecuteNextAsync<Models.Subscriptions>();

            return subscription?.FirstOrDefault()?.SubscriptionId;
        }

        public async Task<List<Models.Subscriptions>> SearchAllSubscriptions()
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var queryCust = client.CreateDocumentQuery<Models.Subscriptions>(collectionUri)
                .AsDocumentQuery();

            var subscriptions = new List<Models.Subscriptions>();

            while (queryCust.HasMoreResults)
            {
                var response = await queryCust.ExecuteNextAsync<Models.Subscriptions>();
                subscriptions.AddRange(response);
            }

            return subscriptions.Any() ? subscriptions : null;
        }


        public async Task<Models.Subscriptions> GetSubscriptionsForCustomerAsync(Guid? customerId, Guid? subscriptionId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            var subscriptionsByIdQuery = client
                ?.CreateDocumentQuery<Models.Subscriptions>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.SubscriptionId == subscriptionId && x.CustomerId == customerId)
                .AsDocumentQuery();

            if (subscriptionsByIdQuery == null)
                return null;

            var customer = await subscriptionsByIdQuery.ExecuteNextAsync<Models.Subscriptions>();

            return customer?.FirstOrDefault();
        }


        public async Task<List<Models.Subscriptions>> GetSubscriptionsForTouchpointAsync(Guid? customerId, string touchpointId, string subcontractorId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            var subscriptionsForTouchpointQuery = client
                ?.CreateDocumentQuery<Models.Subscriptions>(collectionUri)
                .Where(x => x.CustomerId == customerId && x.TouchPointId == touchpointId && x.SubcontractorId == subcontractorId)
                .AsDocumentQuery();

            if (subscriptionsForTouchpointQuery == null)
                return null;

            var subscriptions = new List<Models.Subscriptions>();

            while (subscriptionsForTouchpointQuery.HasMoreResults)
            {
                var response = await subscriptionsForTouchpointQuery.ExecuteNextAsync<Models.Subscriptions>();
                subscriptions.AddRange(response);
            }

            return subscriptions.Any() ? subscriptions : null;
        }
        

        public async Task<ResourceResponse<Document>> CreateSubscriptionsAsync(Models.Subscriptions subscriptions)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, subscriptions);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateSubscriptionsAsync(Models.Subscriptions subscriptions)
        {
            var documentUri = DocumentDBHelper.CreateDocumentUri(subscriptions.SubscriptionId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, subscriptions);

            return response;
        }

    }
}