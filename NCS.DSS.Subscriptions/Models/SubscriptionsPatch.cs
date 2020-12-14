using DFC.Swagger.Standard.Annotations;
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

        [StringLength(10, MinimumLength = 10)]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string LastModifiedBy { get; set; }

        public void SetDefaultValues()
        {
            if (!LastModifiedDate.HasValue)
                LastModifiedDate = DateTime.UtcNow;
        }
    }
}
