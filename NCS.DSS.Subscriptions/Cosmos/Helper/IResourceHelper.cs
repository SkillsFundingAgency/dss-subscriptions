﻿namespace NCS.DSS.Subscriptions.Cosmos.Helper
{
    public interface IResourceHelper
    {
        Task<bool> DoesCustomerExist(Guid customerId);
        Task<Guid?> DoesSubscriptionExist(Guid customerId, string touchpointId);
    }
}