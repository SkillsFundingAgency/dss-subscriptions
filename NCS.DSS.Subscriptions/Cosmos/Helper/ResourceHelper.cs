using System;
using NCS.DSS.Subscriptions.Cosmos.Provider;

namespace NCS.DSS.Subscriptions.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        public bool DoesCustomerExist(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesCustomerExist = documentDbProvider.DoesCustomerResourceExist(customerId);

            return doesCustomerExist;
        }

        public bool DoesSubscriptionExist(Guid customerId, string touchpointId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesSubscriptionExist = documentDbProvider.DoesSubscriptionExist(customerId, touchpointId);

            return doesSubscriptionExist;
        }
    }
}
