using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Subscriptions.Models;

namespace NCS.DSS.Subscriptions.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(ISubscription resource);
    }
}