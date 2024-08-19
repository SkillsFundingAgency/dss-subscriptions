using System;
using System.Threading.Tasks;

namespace NCS.DSS.Subscriptions.GetSubscriptionsByIdHttpTrigger.Service
{
    public interface IGetSubscriptionsByIdHttpTriggerService
    {
        Task<Models.Subscriptions> GetSubscriptionsForCustomerAsync(Guid customerId, Guid subscriptionId);
    }
}