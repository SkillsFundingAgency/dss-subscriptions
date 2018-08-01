using System;
using System.Threading.Tasks;

namespace NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Service
{
    public interface IPatchSubscriptionsHttpTriggerService
    {
        Task<Models.Subscriptions> UpdateAsync(Models.Subscriptions subscriptions, Models.SubscriptionsPatch subscriptionsPatch);
        Task<Models.Subscriptions> GetSubscriptionsForCustomerAsync(Guid customerId, Guid subscriptionId);
    }
}