using System;

namespace NCS.DSS.Subscriptions.Models
{
    public interface ISubscription
    {
        Boolean? Subscribe { get; set; }
        DateTime? LastModifiedDate { get; set; }
        string LastModifiedBy { get; set; }

        void SetDefaultValues();
    }
}
 