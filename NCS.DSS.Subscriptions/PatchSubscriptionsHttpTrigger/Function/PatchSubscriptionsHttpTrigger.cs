using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Subscriptions.Annotations;
using NCS.DSS.Subscriptions.Cosmos.Helper;
using NCS.DSS.Subscriptions.Helpers;
using NCS.DSS.Subscriptions.Ioc;
using NCS.DSS.Subscriptions.Models;
using NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Service;
using NCS.DSS.Subscriptions.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Function
{
    public static class PatchSubscriptionsHttpTrigger
    {
        [FunctionName("Patch")]
        [ResponseType(typeof(Models.SubscriptionsPatch))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Subscriptions Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Subscriptions does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Subscriptions validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to update an existing subscriptions.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Subscriptions/{subscriptionId}")]HttpRequestMessage req, TraceWriter log, string customerId, string subscriptionId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IValidate validate,
            [Inject]IPatchSubscriptionsHttpTriggerService subscriptionsPatchService)
        {
            log.Info("C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(subscriptionId, out var subscriptionsGuid))
                return HttpResponseMessageHelper.BadRequest(subscriptionsGuid);

            SubscriptionsPatch subscriptionsPatchRequest;

            try
            {
                subscriptionsPatchRequest = await httpRequestMessageHelper.GetSubscriptionsFromRequest<SubscriptionsPatch>(req);
            }
            catch (JsonSerializationException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (subscriptionsPatchRequest == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);
           
            var errors = validate.ValidateResource(subscriptionsPatchRequest);

            if (errors != null && errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity(errors);
           
            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);
           
            var subscriptions = await subscriptionsPatchService.GetSubscriptionsForCustomerAsync(customerGuid, subscriptionsGuid);

            if (subscriptions == null)
                return HttpResponseMessageHelper.NoContent(subscriptionsGuid);

           var updatedSubscriptions = await subscriptionsPatchService.UpdateAsync(subscriptions, subscriptionsPatchRequest);

            return updatedSubscriptions == null ? 
                HttpResponseMessageHelper.BadRequest(subscriptionsGuid) :
                HttpResponseMessageHelper.Ok(updatedSubscriptions);
        }
    }
}