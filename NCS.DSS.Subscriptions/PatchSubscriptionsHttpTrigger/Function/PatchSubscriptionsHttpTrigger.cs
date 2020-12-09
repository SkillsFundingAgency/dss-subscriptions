using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Subscriptions.Annotations;
using NCS.DSS.Subscriptions.Cosmos.Helper;
using NCS.DSS.Subscriptions.Helpers;
using NCS.DSS.Subscriptions.Models;
using NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Service;
using NCS.DSS.Subscriptions.Validation;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Function
{
    public class PatchSubscriptionsHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly IValidate _validate;
        private readonly IPatchSubscriptionsHttpTriggerService _subscriptionsPatchService;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;

        public PatchSubscriptionsHttpTrigger(
           IResourceHelper resourceHelper,
           IHttpRequestHelper httpRequestMessageHelper,
           IValidate validate,
           IPatchSubscriptionsHttpTriggerService subscriptionsPatchService,
           IHttpResponseMessageHelper httpResponseMessageHelper)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _validate = validate;
            _subscriptionsPatchService = subscriptionsPatchService;
            _httpResponseMessageHelper = httpResponseMessageHelper;
        }

        [FunctionName("Patch")]
        [ProducesResponseType(typeof(Models.Subscriptions), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Subscriptions Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Subscriptions does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Subscriptions validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to update an existing subscriptions.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Subscriptions/{subscriptionId}")] HttpRequest req, ILogger log, string customerId, string subscriptionId)
        {
            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("C# HTTP trigger function processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return _httpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(subscriptionId, out var subscriptionsGuid))
                return _httpResponseMessageHelper.BadRequest(subscriptionsGuid);

            SubscriptionsPatch subscriptionsPatchRequest;

            try
            {
                subscriptionsPatchRequest = await _httpRequestMessageHelper.GetResourceFromRequest<SubscriptionsPatch>(req);
            }
            catch (JsonSerializationException ex)
            {
                return _httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (subscriptionsPatchRequest == null)
                return _httpResponseMessageHelper.UnprocessableEntity(req);

            subscriptionsPatchRequest.LastModifiedBy = touchpointId;
           
            var errors = _validate.ValidateResource(subscriptionsPatchRequest);

            if (errors != null && errors.Any())
                return _httpResponseMessageHelper.UnprocessableEntity(errors);
           
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return _httpResponseMessageHelper.NoContent(customerGuid);
           
            var subscriptions = await _subscriptionsPatchService.GetSubscriptionsForCustomerAsync(customerGuid, subscriptionsGuid);

            if (subscriptions == null)
                return _httpResponseMessageHelper.NoContent(subscriptionsGuid);

           var updatedSubscriptions = await _subscriptionsPatchService.UpdateAsync(subscriptions, subscriptionsPatchRequest);

            return updatedSubscriptions == null ?
                _httpResponseMessageHelper.BadRequest(subscriptionsGuid) :
                _httpResponseMessageHelper.Ok(JsonHelper.SerializeObject(updatedSubscriptions));
        }
    }
}