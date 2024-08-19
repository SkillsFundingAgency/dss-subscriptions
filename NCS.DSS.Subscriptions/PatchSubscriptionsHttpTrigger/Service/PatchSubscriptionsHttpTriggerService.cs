using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Subscriptions.Cosmos.Provider;

namespace NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Service
{
    public class PatchSubscriptionsHttpTriggerService : IPatchSubscriptionsHttpTriggerService
    {       
        public async Task<Models.Subscriptions> UpdateAsync(Models.Subscriptions subscriptions, Models.SubscriptionsPatch subscriptionsPatch)
        {
            if (subscriptions == null)
                return null;

            if (!subscriptionsPatch.LastModifiedDate.HasValue)
                subscriptionsPatch.LastModifiedDate = DateTime.Now;

            subscriptions.Patch(subscriptionsPatch);

            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.UpdateSubscriptionsAsync(subscriptions);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? subscriptions : null;
        }

        public async Task<Models.Subscriptions> GetSubscriptionsForCustomerAsync(Guid customerId, Guid subscriptionId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var subscriptions = await documentDbProvider.GetSubscriptionsForCustomerAsync(customerId, subscriptionId);
            
            return subscriptions;
        }
    }
}