using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Subscriptions.Cosmos.Helper;
using NCS.DSS.Subscriptions.GetSubscriptionsByIdHttpTrigger.Service;
using NCS.DSS.Subscriptions.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Subscriptions.GetSubscriptionsByIdHttpTrigger.Function
{
    public class GetSubscriptionsByIdHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IGetSubscriptionsByIdHttpTriggerService _getSubscriptionsByIdService;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;

        public GetSubscriptionsByIdHttpTrigger(IResourceHelper resourceHelper, IGetSubscriptionsByIdHttpTriggerService getSubscriptionsByIdService, IHttpResponseMessageHelper httpResponseMessageHelper)
        {
            _resourceHelper = resourceHelper;
            _getSubscriptionsByIdService = getSubscriptionsByIdService;
            _httpResponseMessageHelper = httpResponseMessageHelper;
        }

        [FunctionName("GetById")]
        [ProducesResponseType(typeof(Models.Subscriptions), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Subscriptions found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Subscriptions does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve a single subscriptions with a given SubscriptionsId for an individual customer.")]
        [Disable]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Subscriptions/{subscriptionId}")]HttpRequest req, TraceWriter log, string customerId, string subscriptionId)
        {
            log.Info("C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid))
                return _httpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(subscriptionId, out var subscriptionsGuid))
                return _httpResponseMessageHelper.BadRequest(subscriptionsGuid);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return _httpResponseMessageHelper.NoContent(customerGuid);

            var subscriptions = await _getSubscriptionsByIdService.GetSubscriptionsForCustomerAsync(customerGuid, subscriptionsGuid);

            return subscriptions == null ?
                _httpResponseMessageHelper.NoContent(subscriptionsGuid) :
                _httpResponseMessageHelper.Ok(JsonHelper.SerializeObject(subscriptions));
        }
    }
}