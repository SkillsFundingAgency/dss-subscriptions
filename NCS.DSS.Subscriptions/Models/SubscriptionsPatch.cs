using NCS.DSS.Subscriptions.Annotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Subscriptions.Models
{
    public class SubscriptionsPatch : ISubscription
    {

        [Display(Description = "Indicator to register an interest in changes to the given customer record.  true indicates subscribe, false is unsubscribe")]
        [Example(Description = "true/false")]
        public Boolean? Subscribe { get; set; }

        [Display(Description = "Last modified date & time")]
        [Example(Description = "2018-06-21T14:45:00")]
        public DateTime? LastModifiedDate { get; set; }

        [Display(Description = "Unique identifier of the touchpoint making the change")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        public Guid? LastModifiedBy { get; set; }

        public void SetDefaultValues()
        {
            if (!LastModifiedDate.HasValue)
                LastModifiedDate = DateTime.UtcNow;
        }
    }
}
