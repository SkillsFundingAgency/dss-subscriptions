using System;
using System.Threading.Tasks;

namespace NCS.DSS.Subscriptions.PostSubscriptionsHttpTrigger.Service
{
    public interface IPostSubscriptionsHttpTriggerService
    {
        Task<Models.Subscriptions> CreateAsync(Models.Subscriptions subscriptions);
    }
}