using System;
using System.Threading.Tasks;

namespace NCS.DSS.Subscriptions.Cosmos.Helper
{
    public interface IResourceHelper
    {
        Task<bool> DoesCustomerExist(Guid customerId);
        bool DoesSubscriptionExist(Guid customerId, string touchpointId);
    }
}