﻿using NCS.DSS.Subscriptions.Cosmos.Provider;

namespace NCS.DSS.Subscriptions.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesCustomerExist = await documentDbProvider.DoesCustomerResourceExist(customerId);

            return doesCustomerExist;
        }

        public async Task<Guid?> DoesSubscriptionExist(Guid customerId, string touchpointId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesSubscriptionExist = await documentDbProvider.DoesSubscriptionExist(customerId, touchpointId);

            return doesSubscriptionExist;
        }
    }
}
