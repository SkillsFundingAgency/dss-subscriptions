using NCS.DSS.Subscriptions.Cosmos.Provider;
using System.Net;

namespace NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Service
{
    public class PatchSubscriptionsHttpTriggerService : IPatchSubscriptionsHttpTriggerService
    {
        private readonly ICosmosDBProvider _cosmosDbProvider;
        public PatchSubscriptionsHttpTriggerService(ICosmosDBProvider cosmosDbProvider)
        {
            _cosmosDbProvider = cosmosDbProvider;
        }
        public async Task<Models.Subscriptions> UpdateAsync(Models.Subscriptions subscriptions, Models.SubscriptionsPatch subscriptionsPatch)
        {
            if (subscriptions == null)
                return null;

            if (!subscriptionsPatch.LastModifiedDate.HasValue)
                subscriptionsPatch.LastModifiedDate = DateTime.Now;

            subscriptions.Patch(subscriptionsPatch);

            var response = await _cosmosDbProvider.UpdateSubscriptionsAsync(subscriptions);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? subscriptions : null;
        }

        public async Task<Models.Subscriptions> GetSubscriptionsForCustomerAsync(Guid customerId, Guid subscriptionId)
        {
            var subscriptions = await _cosmosDbProvider.GetSubscriptionsForCustomerAsync(customerId, subscriptionId);

            return subscriptions;
        }
        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            return await _cosmosDbProvider.DoesCustomerResourceExist(customerId); 
        }
    }
}