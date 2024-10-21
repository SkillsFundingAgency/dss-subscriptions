namespace NCS.DSS.Subscriptions.PostSubscriptionsHttpTrigger.Service
{
    public interface IPostSubscriptionsHttpTriggerService
    {
        Task<Models.Subscriptions> CreateAsync(Models.Subscriptions subscriptions);
    }
}