using System;
using System.Threading.Tasks;

namespace NCS.DSS.Subscriptions.GetSubscriptionsForTouchpointHttpTrigger.Service
{
    public interface IGetSubscriptionsForTouchpointHttpTriggerService
    {
        Task<Models.Subscriptions> GetSubscriptionsForTouchpointAsync(Guid customerId, string TouchpointId);
    }
}