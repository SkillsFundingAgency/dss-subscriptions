using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCS.DSS.Subscriptions.Models
{
    public class Subscription
    {
        public Guid SubscriptionId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid TouchPointId { get; set; }
        public Boolean Subscribe { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public Guid LastModifiedBy { get; set; }
    }
}
