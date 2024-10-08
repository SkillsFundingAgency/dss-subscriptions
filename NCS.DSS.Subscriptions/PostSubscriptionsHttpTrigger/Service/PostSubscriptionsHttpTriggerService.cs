﻿using NCS.DSS.Subscriptions.Cosmos.Provider;
using System.Net;

namespace NCS.DSS.Subscriptions.PostSubscriptionsHttpTrigger.Service
{
    public class PostSubscriptionsHttpTriggerService : IPostSubscriptionsHttpTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;
        public PostSubscriptionsHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }
        public async Task<Models.Subscriptions> CreateAsync(Models.Subscriptions subscriptions)
        {
            if (subscriptions == null)
                return null;

            var subscriptionId = Guid.NewGuid();
            subscriptions.SubscriptionId = subscriptionId;

            if (!subscriptions.LastModifiedDate.HasValue)
                subscriptions.LastModifiedDate = DateTime.Now;

            var response = await _documentDbProvider.CreateSubscriptionsAsync(subscriptions);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : (Guid?)null;
        }
    }
}