namespace NCS.DSS.Subscriptions.Models
{
    public class SubscriptionsConfigurationSettings
    {
        public string SubscriptionsConnectionString => string.Format("AccountEndpoint={0};AccountKey={1};", Endpoint, Key);
        public string CollectionId { get; set; }
        public string CustomerCollectionId { get; set; }
        public string CustomerDatabaseId { get; set; }
        public string DatabaseId { get; set; }
        public string Endpoint { get; set; }
        public string Key { get; set; }
       
    }
}
