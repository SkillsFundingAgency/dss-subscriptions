using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Logging;
using NCS.DSS.Subscriptions.Cosmos.Helper;
using NCS.DSS.Subscriptions.GetSubscriptionsByIdHttpTrigger.Service;
using NCS.DSS.Subscriptions.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

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

        //[Function("GetById")]
        //[ProducesResponseType(typeof(Models.Subscriptions), (int)HttpStatusCode.OK)]
        //[Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Subscriptions found", ShowSchema = true)]
        //[Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Subscriptions does not exist", ShowSchema = false)]
        //[Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        //[Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        //[Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        //[Display(Name = "Get", Description = "Ability to retrieve a single subscriptions with a given SubscriptionsId for an individual customer.")]
        //[Disable]
        //public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Subscriptions/{subscriptionId}")]HttpRequest req, ILogger log, string customerId, string subscriptionId)
        //{
        //    log.LogInformation("C# HTTP trigger function processed a request.");

        //    if (!Guid.TryParse(customerId, out var customerGuid))
        //    {
        //        log.LogWarning($"GetSubscriptionsByIdHttpTrigger customerId {customerId} BadRequest");
        //        return _httpResponseMessageHelper.BadRequest(customerGuid);
        //    }

        //    if (!Guid.TryParse(subscriptionId, out var subscriptionsGuid))
        //    {
        //        log.LogWarning($"GetSubscriptionsByIdHttpTrigger subscriptionId {subscriptionId} BadRequest");
        //        return _httpResponseMessageHelper.BadRequest(subscriptionsGuid);
        //    } 

        //    var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

        //    if (!doesCustomerExist)
        //    {
        //        log.LogWarning($"GetSubscriptionsByIdHttpTrigger customerGuid {customerGuid} NoContent");
        //        return _httpResponseMessageHelper.NoContent(customerGuid);
        //    }

        //    var subscriptions = await _getSubscriptionsByIdService.GetSubscriptionsForCustomerAsync(customerGuid, subscriptionsGuid);
        //    log.LogInformation($"GetSubscriptionsByIdHttpTrigger _getSubscriptionsByIdService customerGuid {customerGuid} subscriptionsGuid {subscriptionsGuid}");

        //    return subscriptions == null ?
        //        _httpResponseMessageHelper.NoContent(subscriptionsGuid) :
        //        _httpResponseMessageHelper.Ok(JsonHelper.SerializeObject(subscriptions));
        //}
    }
}