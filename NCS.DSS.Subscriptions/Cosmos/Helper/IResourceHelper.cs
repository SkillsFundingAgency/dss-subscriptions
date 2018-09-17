using System;

namespace NCS.DSS.Subscriptions.Cosmos.Helper
{
    public interface IResourceHelper
    {
        bool DoesCustomerExist(Guid customerId);
        bool DoesSubscriptionExist(Guid customerId, string touchpointId);
    }
}