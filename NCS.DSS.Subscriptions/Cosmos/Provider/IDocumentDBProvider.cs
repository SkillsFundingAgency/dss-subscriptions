using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Subscriptions.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        Task<Guid?> DoesSubscriptionExist(Guid customerId, string touchpointId);
        Task<List<Models.Subscriptions>> SearchAllSubscriptions();
        Task<Models.Subscriptions> GetSubscriptionsForCustomerAsync(Guid? customerId, Guid? subscriptionId);
        Task<ResourceResponse<Document>> CreateSubscriptionsAsync(Models.Subscriptions subscriptions);
        Task<ResourceResponse<Document>> UpdateSubscriptionsAsync(Models.Subscriptions subscriptions);
    }
}