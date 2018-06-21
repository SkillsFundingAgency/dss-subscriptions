using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Description;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Subscriptions.Annotations;

namespace NCS.DSS.Subscriptions.GetSubscriptionHttpTrigger
{ 
    public static class GetSubscriptionHttpTrigger
    {
        [FunctionName("GET")]
        [SubscriptionsResponse(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Subscription Details Retrieved", ShowSchema = true)]
        [SubscriptionsResponse(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Unable to Retrieve Subscription Details", ShowSchema = false)]
        [SubscriptionsResponse(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Forbidden", ShowSchema = false)]
        [ResponseType(typeof(Models.Subscription))]
        [Display(Name = "Get", Description = "Ability to get a session object for a given customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers/subscriptions/")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function Get Subscription processed a request.");

            var service = new GetSubscriptionHttpTriggerService();
            var values = await service.GetSubscriptions();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(values),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }

    }

}
