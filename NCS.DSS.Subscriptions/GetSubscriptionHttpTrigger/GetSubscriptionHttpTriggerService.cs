﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCS.DSS.Subscriptions;

namespace NCS.DSS.Subscriptions.GetSubscriptionHttpTrigger
{
    public class GetSubscriptionHttpTriggerService
    {
        public async Task<List<Models.Subscription>> GetSubscriptions()
        {
            var result = GenerateSampleData();
            return await Task.FromResult(result);
        }

        private List<Models.Subscription> GenerateSampleData()
        {
            var cList = new List<Models.Subscription>();

            cList.Add(new Models.Subscription { CustomerId = Guid.NewGuid(), LastModifiedBy = Guid.NewGuid(), LastModifiedDate = DateTime.Now, Subscribe = true, SubscriptionId = Guid.NewGuid(), TouchPointId = Guid.NewGuid() });
            cList.Add(new Models.Subscription { CustomerId = Guid.NewGuid(), LastModifiedBy = Guid.NewGuid(), LastModifiedDate = DateTime.Now, Subscribe = true, SubscriptionId = Guid.NewGuid(), TouchPointId = Guid.NewGuid() });
            cList.Add(new Models.Subscription { CustomerId = Guid.NewGuid(), LastModifiedBy = Guid.NewGuid(), LastModifiedDate = DateTime.Now, Subscribe = true, SubscriptionId = Guid.NewGuid(), TouchPointId = Guid.NewGuid() });
            cList.Add(new Models.Subscription { CustomerId = Guid.NewGuid(), LastModifiedBy = Guid.NewGuid(), LastModifiedDate = DateTime.Now, Subscribe = true, SubscriptionId = Guid.NewGuid(), TouchPointId = Guid.NewGuid() });
            cList.Add(new Models.Subscription { CustomerId = Guid.NewGuid(), LastModifiedBy = Guid.NewGuid(), LastModifiedDate = DateTime.Now, Subscribe = true, SubscriptionId = Guid.NewGuid(), TouchPointId = Guid.NewGuid() });
            cList.Add(new Models.Subscription { CustomerId = Guid.NewGuid(), LastModifiedBy = Guid.NewGuid(), LastModifiedDate = DateTime.Now, Subscribe = true, SubscriptionId = Guid.NewGuid(), TouchPointId = Guid.NewGuid() });


            return cList;
        }


    }
}
