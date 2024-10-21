using NCS.DSS.Subscriptions.Models;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Subscriptions.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(ISubscription resource);
        SubscriptionError ValidateResultForDuplicateSubscriptionId(Guid subscriptionId);
    }
}