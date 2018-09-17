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

        public bool DoesSubscriptionExist(Guid customerId, string touchpointId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var subscriptionQuery = client.CreateDocumentQuery<Models.Subscriptions>(collectionUri, new FeedOptions() { MaxItemCount = 1 });
            return subscriptionQuery.Where(x => x.CustomerId == customerId && 
                                                x.TouchPointId == touchpointId &&
                                                x.Subscribe == true)
                .Select(x => x.SubscriptionId).AsEnumerable().Any();
        }

        public async Task<List<Models.Subscriptions>> SearchAllSubscriptions()
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var queryCust = client.CreateDocumentQuery<Models.Subscriptions>(collectionUri)
                .AsDocumentQuery();

            var Subscriptions = new List<Models.Subscriptions>();

            while (queryCust.HasMoreResults)
            {
                var response = await queryCust.ExecuteNextAsync<Models.Subscriptions>();
                Subscriptions.AddRange(response);
            }

            return Subscriptions.Any() ? Subscriptions : null;
        }


        public async Task<Models.Subscriptions> GetSubscriptionsForCustomerAsync(Guid? customerId, Guid? subscriptionId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            var SubscriptionsByIdQuery = client
                ?.CreateDocumentQuery<Models.Subscriptions>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.SubscriptionId == subscriptionId && x.CustomerId == customerId)
                .AsDocumentQuery();

            if (SubscriptionsByIdQuery == null)
                return null;

            var Customer = await SubscriptionsByIdQuery.ExecuteNextAsync<Models.Subscriptions>();

            return Customer?.FirstOrDefault();
        }

                
        public async Task<ResourceResponse<Document>> CreateSubscriptionsAsync(Models.Subscriptions subscriptions)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, subscriptions);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateSubscriptionsAsync(Models.Subscriptions subscriptions)
        {
            var documentUri = _documentDbHelper.CreateDocumentUri(subscriptions.SubscriptionId);

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, subscriptions);

            return response;
        }

    }
}