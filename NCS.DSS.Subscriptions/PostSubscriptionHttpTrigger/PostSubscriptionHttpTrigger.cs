using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Web.Http.Description;

namespace NCS.DSS.Subscriptions.PostSubscriptionHttpTrigger
{ 
    public static class PostSubscriptionHttpTrigger
    {
        [FunctionName("POST")]
        [ResponseType(typeof(Models.Subscription))]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "customers/{customerId}/subscriptions/{subscriptionid}")]HttpRequestMessage req, TraceWriter log, string subscriptionid)
        {
            log.Info("C# HTTP trigger function Post Subscription processed a request.");
            
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject("Post successful for subscription with id: " + subscriptionid),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }

    }

}