using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Subscriptions.Models;

namespace NCS.DSS.Subscriptions.Validation
{
    public class Validate : IValidate
    {
        public List<ValidationResult> ValidateResource(ISubscription resource)
        {
            var context = new ValidationContext(resource, null, null);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(resource, context, results, true);
            ValidateSubscriptionRules(resource, results);
            return results;
        }

        private void ValidateSubscriptionRules(ISubscription subscriptionrResource, List<ValidationResult> results)
        {
            if (subscriptionrResource == null)
                return;
            
            if (subscriptionrResource.LastModifiedDate.HasValue && subscriptionrResource.LastModifiedDate.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Last Modified Date must be less the current date/time", new[] { "LastModifiedDate" }));

        }

    }
}
