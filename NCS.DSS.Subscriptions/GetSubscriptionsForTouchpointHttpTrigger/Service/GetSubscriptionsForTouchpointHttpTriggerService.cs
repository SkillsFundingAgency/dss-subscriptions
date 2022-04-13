using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NCS.DSS.Subscriptions.Cosmos.Provider;

namespace NCS.DSS.Subscriptions.GetSubscriptionsForTouchpointHttpTrigger.Service
{
    public class GetSubscriptionsForTouchpointHttpTriggerService : IGetSubscriptionsForTouchpointHttpTriggerService
    {
        public async Task<List<Models.Subscriptions>> GetSubscriptionsForTouchpointAsync(Guid customerId, string TouchpointId, string subcontractorId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var subscriptions = await documentDbProvider.GetSubscriptionsForTouchpointAsync(customerId, TouchpointId, subcontractorId);

            return subscriptions;
        }
    }
}