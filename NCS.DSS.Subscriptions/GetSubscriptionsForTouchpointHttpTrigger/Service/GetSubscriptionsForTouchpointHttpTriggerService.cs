using NCS.DSS.Subscriptions.Cosmos.Provider;

namespace NCS.DSS.Subscriptions.GetSubscriptionsForTouchpointHttpTrigger.Service
{
    public class GetSubscriptionsForTouchpointHttpTriggerService : IGetSubscriptionsForTouchpointHttpTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;
        public GetSubscriptionsForTouchpointHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }
        public async Task<List<Models.Subscriptions>> GetSubscriptionsForTouchpointAsync(Guid customerId, string TouchpointId)
        {
            var subscriptions = await _documentDbProvider.GetSubscriptionsForTouchpointAsync(customerId, TouchpointId);

            return subscriptions;
        }
    }
}