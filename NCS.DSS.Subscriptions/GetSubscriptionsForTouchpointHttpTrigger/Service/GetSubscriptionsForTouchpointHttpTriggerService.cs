using NCS.DSS.Subscriptions.Cosmos.Provider;

namespace NCS.DSS.Subscriptions.GetSubscriptionsForTouchpointHttpTrigger.Service
{
    public class GetSubscriptionsForTouchpointHttpTriggerService : IGetSubscriptionsForTouchpointHttpTriggerService
    {
        private readonly ICosmosDBProvider _cosmosDbProvider;
        public GetSubscriptionsForTouchpointHttpTriggerService(ICosmosDBProvider cosmosDbProvider)
        {
            _cosmosDbProvider = cosmosDbProvider;
        }
        public async Task<List<Models.Subscriptions>> GetSubscriptionsForTouchpointAsync(Guid customerId, string TouchpointId)
        {
            var subscriptions = await _cosmosDbProvider.GetSubscriptionsForTouchpointAsync(customerId, TouchpointId);

            return subscriptions;
        }
        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            return await _cosmosDbProvider.DoesCustomerResourceExist(customerId);
        }
    }
}