using System;

namespace NCS.DSS.Subscriptions.Models
{
    public interface ISubscription
    {
        Boolean? Subscribe { get; set; }
        DateTime? LastModifiedDate { get; set; }
        Guid? LastModifiedBy { get; set; }

        void SetDefaultValues();
    }
}
 