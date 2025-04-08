using Microsoft.Azure.Cosmos;

namespace NCS.DSS.Subscriptions.Cosmos.Provider
{
    public interface ICosmosDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        Task<Guid?> DoesSubscriptionExist(Guid customerId, string touchpointId);
        Task<List<Models.Subscriptions>> SearchAllSubscriptions();
        Task<Models.Subscriptions> GetSubscriptionsForCustomerAsync(Guid? customerId, Guid? subscriptionId);
        Task<ItemResponse<Models.Subscriptions>> CreateSubscriptionsAsync(Models.Subscriptions subscriptions);
        Task<ItemResponse<Models.Subscriptions>> UpdateSubscriptionsAsync(Models.Subscriptions subscriptions);
        Task<List<Models.Subscriptions>> GetSubscriptionsForTouchpointAsync(Guid? customerId, string touchpointId);
    }
}