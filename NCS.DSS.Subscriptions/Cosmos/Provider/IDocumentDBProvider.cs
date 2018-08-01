using System;
using System.Threading.Tasks;

namespace NCS.DSS.Subscriptions.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<Models.Subscriptions> GetSubscriptionsForCustomerAsync(Guid? customerId, Guid? subscriptionId);

    }
}