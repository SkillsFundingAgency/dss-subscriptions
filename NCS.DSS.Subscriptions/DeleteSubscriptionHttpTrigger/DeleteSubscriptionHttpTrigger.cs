using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Web.Http.Description;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Subscriptions.Annotations;

namespace NCS.DSS.Subscriptions.DeleteSubscriptionHttpTrigger
{ 
    public static class DeleteSubscriptionHttpTrigger
    {
        [FunctionName("DELETE")]
        [SubscriptionsResponse(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Subscription Details Deleted", ShowSchema = true)]
        [SubscriptionsResponse(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Unable to Delete Subscription Details", ShowSchema = false)]
        [SubscriptionsResponse(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Forbidden", ShowSchema = false)]
        [ResponseType(typeof(Models.Subscription))]
        [Display(Name = "Delete", Description = "Ability to delete a subscriptions object for a given customer.")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "customers/{customerId}/subscriptions/{subscriptionid}")]HttpRequestMessage req, TraceWriter log, string customerId, string subscriptionid)
        {
            log.Info("C# HTTP trigger function Delete Subscription processed a request.");
            
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject("Delete successful for subscription with id: " + subscriptionid),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }

    }

}