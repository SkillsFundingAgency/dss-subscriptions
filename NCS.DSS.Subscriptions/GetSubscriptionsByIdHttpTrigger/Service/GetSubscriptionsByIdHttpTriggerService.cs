using System;
using System.Threading.Tasks;
using NCS.DSS.Subscriptions.Cosmos.Provider;

namespace NCS.DSS.Subscriptions.GetSubscriptionsByIdHttpTrigger.Service
{
    public class GetSubscriptionsByIdHttpTriggerService : IGetSubscriptionsByIdHttpTriggerService
    {
        public async Task<Models.Subscriptions> GetSubscriptionsForCustomerAsync(Guid customerId, Guid subscriptionId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var subscriptions = await documentDbProvider.GetSubscriptionsForCustomerAsync(customerId, subscriptionId);

            return subscriptions;
        }
    }
}