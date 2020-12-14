using DFC.Swagger.Standard.Annotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Subscriptions.Models
{
    public class Subscriptions : ISubscription
    {
        [Display(Description = "Unique identifier of a subscription")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public Guid? SubscriptionId { get; set; }

        [Display(Description = "Unique identifier of a customer")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        public Guid? CustomerId { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string TouchPointId { get; set; }

        [Display(Description = "Indicator to register an interest in changes to the given customer record.  true indicates subscribe, false is unsubscribe")]
        [Example(Description = "true/false")]
        public bool? Subscribe { get; set; }

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

        public void SetIds(Guid customerId, string touchpointId)
        {
            SubscriptionId = Guid.NewGuid();
            CustomerId = customerId;
            TouchPointId = touchpointId;
            LastModifiedBy = touchpointId;
        }

        public void Patch(SubscriptionsPatch subscriptionsPatch)
        {
            if (subscriptionsPatch == null)
                return;

            if (subscriptionsPatch.Subscribe.HasValue)
                this.Subscribe = subscriptionsPatch.Subscribe;

            if (subscriptionsPatch.LastModifiedDate != null)
                this.LastModifiedDate= subscriptionsPatch.LastModifiedDate;

            if (!string.IsNullOrEmpty(subscriptionsPatch.LastModifiedBy))
                this.LastModifiedBy = subscriptionsPatch.LastModifiedBy;
        }



    }



}
