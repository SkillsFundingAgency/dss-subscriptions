using NCS.DSS.Subscriptions.Cosmos.Provider;
using System.Net;

namespace NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Service
{
    public class PatchSubscriptionsHttpTriggerService : IPatchSubscriptionsHttpTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;
        public PatchSubscriptionsHttpTriggerService(IDocumentDBProvider documentDbProvider) { 
            _documentDbProvider = documentDbProvider;
        }
        public async Task<Models.Subscriptions> UpdateAsync(Models.Subscriptions subscriptions, Models.SubscriptionsPatch subscriptionsPatch)
        {
            if (subscriptions == null)
                return null;

            if (!subscriptionsPatch.LastModifiedDate.HasValue)
                subscriptionsPatch.LastModifiedDate = DateTime.Now;

            subscriptions.Patch(subscriptionsPatch);

            var response = await _documentDbProvider.UpdateSubscriptionsAsync(subscriptions);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? subscriptions : null;
        }

        public async Task<Models.Subscriptions> GetSubscriptionsForCustomerAsync(Guid customerId, Guid subscriptionId)
        {
            var subscriptions = await _documentDbProvider.GetSubscriptionsForCustomerAsync(customerId, subscriptionId);

            return subscriptions;
        }
    }
}