using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Subscriptions.Models;
using System.Net;

namespace NCS.DSS.Subscriptions.Cosmos.Provider
{
    public class CosmosDBProvider : ICosmosDBProvider
    {
        private readonly Container _container;
        private readonly Container _customerContainer;
        private readonly PartitionKey _partitionKey = PartitionKey.None;
        private readonly ILogger<CosmosDBProvider> _logger;
        public CosmosDBProvider(
            CosmosClient cosmosClient,
            IOptions<SubscriptionsConfigurationSettings> configOptions,
            ILogger<CosmosDBProvider> logger)
        {
            var config = configOptions.Value;
            _container = GetContainer(cosmosClient, config.DatabaseId, config.CollectionId);
            _customerContainer = GetContainer(cosmosClient, config.CustomerDatabaseId, config.CustomerCollectionId);
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

        public async Task<Guid?> DoesSubscriptionExist(Guid customerId, string touchpointId)
        {
            try
            {
                var querySubs = _container.GetItemLinqQueryable<Models.Subscriptions>()
                                    .Where(x => x.CustomerId == customerId && x.TouchPointId == touchpointId).ToFeedIterator();

                while (querySubs.HasMoreResults)
                {
                    var response = await querySubs.ReadNextAsync();
                    if (response != null && response.Resource.Any())
                    {
                        _logger.LogInformation("Subscription Record found with Touchpoint ID {touchpointId} in Cosmos DB for Customer with ID {CustomerID}", touchpointId, customerId);
                        return response.Resource.FirstOrDefault().SubscriptionId;
                    }
                }
                _logger.LogWarning("No Subscription found with Touchpoint ID {touchpointId} and Customer ID {CustomerID} in Cosmos DB", touchpointId, customerId);
                return null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to find the Subscription Record in Cosmos DB {CustomerID}. Exception {Exception}", customerId, ce.Message);
                throw;
            }
        }

        public async Task<List<Models.Subscriptions>> SearchAllSubscriptions()
        {
            try
            {
                var querySubs = _container.GetItemLinqQueryable<Models.Subscriptions>().ToFeedIterator();

                while (querySubs.HasMoreResults)
                {
                    var response = await querySubs.ReadNextAsync();
                    if (response != null && response.Resource.Any())
                    {
                        _logger.LogInformation("Retrieved All Subscriptions in Cosmos Database");
                        return response.Resource.ToList();
                    }
                }
                _logger.LogWarning("No Subscriptions found in Cosmos DB");
                return null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to retrieve all Subscriptions in Cosmos DB . Exception {Exception}", ce.Message);
                throw;
            }
        }


        public async Task<Models.Subscriptions> GetSubscriptionsForCustomerAsync(Guid? customerId, Guid? subscriptionId)
        {
            try
            {
                var querySubs = _container.GetItemLinqQueryable<Models.Subscriptions>()
                                    .Where(x => x.CustomerId == customerId && x.SubscriptionId == subscriptionId).ToFeedIterator();

                while (querySubs.HasMoreResults)
                {
                    var response = await querySubs.ReadNextAsync();
                    if (response != null && response.Resource.Any())
                    {
                        _logger.LogInformation("Subscription Record found with ID {subscriptionId} in Cosmos DB for Customer with ID {CustomerID}", subscriptionId, customerId);
                        return response.Resource.FirstOrDefault();
                    }
                }
                _logger.LogWarning("No Subscription found with ID {subscriptionId} and Customer ID {CustomerID} in Cosmos DB", subscriptionId, customerId);
                return null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to find the Subscription Record in Cosmos DB {CustomerID}. Exception {Exception}", customerId, ce.Message);
                throw;
            }
        }


        public async Task<List<Models.Subscriptions>> GetSubscriptionsForTouchpointAsync(Guid? customerId, string touchpointId)
        {
            try
            {
                var querySubs = _container.GetItemLinqQueryable<Models.Subscriptions>()
                                    .Where(x => x.CustomerId == customerId && x.TouchPointId == touchpointId).ToFeedIterator();

                while (querySubs.HasMoreResults)
                {
                    var response = await querySubs.ReadNextAsync();
                    if (response != null && response.Resource.Any())
                    {
                        _logger.LogInformation("Subscriptions found with Touchpoint ID {touchpointId} in Cosmos DB for Customer with ID {CustomerID}", touchpointId, customerId);
                        return response.Resource.ToList();
                    }
                }
                _logger.LogWarning("No Subscriptions found with Touchpoint ID {touchpointId} and Customer ID {CustomerID} in Cosmos DB", touchpointId, customerId);
                return null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to find the Subscriptions in Cosmos DB {CustomerID}. Exception {Exception}", customerId, ce.Message);
                throw;
            }
        }


        public async Task<ItemResponse<Models.Subscriptions>> CreateSubscriptionsAsync(Models.Subscriptions subscriptions)
        {
            try
            {
                var response = await _container.CreateItemAsync(subscriptions, null);
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    _logger.LogInformation("Subscription Record Created in Cosmos DB for {SubscriptionId}", subscriptions.SubscriptionId);
                }
                else
                {
                    _logger.LogError("Failed and returned {StatusCode} to CreateSubscription Record in Cosmos DB for {SubscriptionId}", response.StatusCode, subscriptions.SubscriptionId);
                }
                return response;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to CreateSubscription Record in Cosmos DB {SubscriptionId}. Exception {Exception}.", subscriptions.SubscriptionId, ce.Message);
                throw;
            }

        }

        public async Task<ItemResponse<Models.Subscriptions>> UpdateSubscriptionsAsync(Models.Subscriptions subscriptions)
        {
            var subscriptionId = subscriptions.SubscriptionId;
            try
            {                
                var response = await _container.ReplaceItemAsync(subscriptions, subscriptionId.ToString());
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation("Session Record Updated in Cosmos DB for {SubscriptionId}", subscriptionId);
                }
                else
                {
                    _logger.LogError("Failed and returned {StatusCode} to Update Session Record in Cosmos DB for {SubscriptionId}", response.StatusCode, subscriptionId);
                }
                return response;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to Update Session Record in Cosmos DB {SubscriptionId}. Exception {Exception}.", subscriptionId, ce.Message);
                throw;
            }
        }

    }
}