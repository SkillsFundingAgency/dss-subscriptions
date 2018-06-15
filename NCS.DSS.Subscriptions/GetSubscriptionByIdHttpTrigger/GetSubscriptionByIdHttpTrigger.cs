using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Web.Http.Description;

namespace NCS.DSS.Subscriptions.GetSubscriptionByIdHttpTrigger
{ 
    public static class GetSubscriptionByIdHttpTrigger
    {
        [FunctionName("GETByID")]
        [ResponseType(typeof(Models.Subscription))]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers/{customerId}/subscriptions/{subscriptionid}")]HttpRequestMessage req, TraceWriter log, string subscriptionid)
        {
            log.Info("C# HTTP trigger function Get Subscription By Id processed a request.");

            if (!Guid.TryParse(subscriptionid, out var subscriptionGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(subscriptionid),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            var service = new GetSubscriptionByIdHttpTriggerService();
            var values = await service.GetSubscriptions(subscriptionGuid);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(values),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }

    }

}