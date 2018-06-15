using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Web.Http.Description;

namespace NCS.DSS.Subscriptions.PatchSubscriptionHttpTrigger
{ 
    public static class PatchSubscriptionHttpTrigger
    {
        [FunctionName("PATCH")]
        [ResponseType(typeof(Models.Subscription))]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "Patch", Route = "customers/{customerId}/subscriptions/{subscriptionid}")]HttpRequestMessage req, TraceWriter log, string subscriptionid)
        {
            log.Info("C# HTTP trigger function Patch Subscription processed a request.");
            
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject("Patch successful for subscription with id: " + subscriptionid),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }

    }

}