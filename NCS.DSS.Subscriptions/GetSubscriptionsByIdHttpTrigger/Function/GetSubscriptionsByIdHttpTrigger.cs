using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Description;
using NCS.DSS.Subscriptions.Annotations;
using NCS.DSS.Subscriptions.Cosmos.Helper;
using NCS.DSS.Subscriptions.GetSubscriptionsByIdHttpTrigger.Service;
using NCS.DSS.Subscriptions.Helpers;
using NCS.DSS.Subscriptions.Ioc;

namespace NCS.DSS.Subscriptions.GetSubscriptionsByIdHttpTrigger.Function
{
    public static class GetSubscriptionsByIdHttpTrigger
    {
        [FunctionName("GetById")]
        [ResponseType(typeof(Models.Subscriptions))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Subscriptions found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Subscriptions does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve a single subscriptions with a given SubscriptionsId for an individual customer.")]
        [Disable]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Subscriptions/{subscriptionId}")]HttpRequestMessage req, TraceWriter log, string customerId, string subscriptionId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IGetSubscriptionsByIdHttpTriggerService getSubscriptionsByIdService)
        {
            log.Info("C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(subscriptionId, out var subscriptionsGuid))
                return HttpResponseMessageHelper.BadRequest(subscriptionsGuid);

            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var subscriptions = await getSubscriptionsByIdService.GetSubscriptionsForCustomerAsync(customerGuid, subscriptionsGuid);

            return subscriptions == null ? 
                HttpResponseMessageHelper.NoContent(subscriptionsGuid) :
                HttpResponseMessageHelper.Ok(subscriptions);
        }
    }
}