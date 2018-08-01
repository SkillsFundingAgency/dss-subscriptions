using System;

namespace NCS.DSS.Subscriptions.Cosmos.Helper
{
    public interface IDocumentDBHelper
    {
        Uri CreateDocumentCollectionUri();
        Uri CreateDocumentUri(Guid? subscriptionId);
        Uri CreateCustomerDocumentCollectionUri();

    }
}