namespace NCS.DSS.Subscriptions.PostSubscriptionsHttpTrigger.Service
{
    public interface IPostSubscriptionsHttpTriggerService
    {
        Task<Models.Subscriptions> CreateAsync(Models.Subscriptions subscriptions);
        Task<bool> DoesCustomerExist(Guid customerId);
        Task<Guid?> DoesSubscriptionExist(Guid customerId, string touchpointId);
    }
}