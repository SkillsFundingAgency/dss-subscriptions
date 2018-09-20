using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.Subscriptions.GetSubscriptionsForTouchpointHttpTrigger.Service
{
    public interface IGetSubscriptionsForTouchpointHttpTriggerService
    {
        Task<List<Models.Subscriptions>> GetSubscriptionsForTouchpointAsync(Guid customerId, string TouchpointId);
    }
}