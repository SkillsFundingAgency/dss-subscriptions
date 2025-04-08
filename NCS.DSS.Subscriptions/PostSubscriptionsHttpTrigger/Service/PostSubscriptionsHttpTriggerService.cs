using NCS.DSS.Subscriptions.Cosmos.Provider;
using System.Net;

namespace NCS.DSS.Subscriptions.PostSubscriptionsHttpTrigger.Service
{
    public class PostSubscriptionsHttpTriggerService : IPostSubscriptionsHttpTriggerService
    {
        private readonly ICosmosDBProvider _cosmosDbProvider;
        public PostSubscriptionsHttpTriggerService(ICosmosDBProvider cosmosDbProvider)
        {
            _cosmosDbProvider = cosmosDbProvider;
        }
        public async Task<Models.Subscriptions> CreateAsync(Models.Subscriptions subscriptions)
        {
            if (subscriptions == null)
                return null;

            var subscriptionId = Guid.NewGuid();
            subscriptions.SubscriptionId = subscriptionId;

            if (!subscriptions.LastModifiedDate.HasValue)
                subscriptions.LastModifiedDate = DateTime.Now;

            var response = await _cosmosDbProvider.CreateSubscriptionsAsync(subscriptions);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : (Guid?)null;
        }
        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            return await _cosmosDbProvider.DoesCustomerResourceExist(customerId);
        }
        public async Task<Guid?> DoesSubscriptionExist(Guid customerId,string touchpointId)
        {
            return await _cosmosDbProvider.DoesSubscriptionExist(customerId,touchpointId);
        }
    }
}