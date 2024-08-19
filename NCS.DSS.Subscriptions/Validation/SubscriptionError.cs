namespace NCS.DSS.Subscriptions.Validation
{
    public class SubscriptionError
    {
        public string MemberName { get; set; }
        public string ErrorMessage { get; set; }
        public Guid Id { get; set; }
    }
}
