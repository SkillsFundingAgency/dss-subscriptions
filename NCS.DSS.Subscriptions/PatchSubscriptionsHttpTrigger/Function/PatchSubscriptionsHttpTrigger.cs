using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Subscriptions.Cosmos.Helper;
using NCS.DSS.Subscriptions.Helpers;
using NCS.DSS.Subscriptions.Models;
using NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Service;
using NCS.DSS.Subscriptions.Validation;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Function
{
    public class PatchSubscriptionsHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly IValidate _validate;
        private readonly IPatchSubscriptionsHttpTriggerService _subscriptionsPatchService;
        private readonly ILogger<PatchSubscriptionsHttpTrigger> _loggerHelper;
        private readonly IConvertToDynamic _convertToDynamic;
        public PatchSubscriptionsHttpTrigger(
           IResourceHelper resourceHelper,
           IHttpRequestHelper httpRequestMessageHelper,
           IValidate validate,
           IPatchSubscriptionsHttpTriggerService subscriptionsPatchService,
           IConvertToDynamic convertToDynamic,
           ILogger<PatchSubscriptionsHttpTrigger> loggerHelper)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _validate = validate;
            _subscriptionsPatchService = subscriptionsPatchService;
            _convertToDynamic = convertToDynamic;
            _loggerHelper = loggerHelper;
        }

        [Function("Patch")]
        [ProducesResponseType(typeof(Models.Subscriptions), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Subscriptions Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Subscriptions does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Subscriptions validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to update an existing subscriptions.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Subscriptions/{subscriptionId}")] HttpRequest req, string customerId, string subscriptionId)
        {
            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _loggerHelper.LogWarning($"PatchSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions/{subscriptionId} Unable to locate 'TouchpointId' in request header");
                return new BadRequestResult();
            }

            _loggerHelper.LogInformation("C# HTTP trigger function processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _loggerHelper.LogWarning($"PatchSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions/{subscriptionId} BadRequest CustomerId");
                return new BadRequestObjectResult(customerGuid);
            }

            if (!Guid.TryParse(subscriptionId, out var subscriptionsGuid))
            {
                _loggerHelper.LogWarning($"PatchSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions/{subscriptionId} BadRequest subscriptionId");
                return new BadRequestObjectResult(subscriptionsGuid);
            }

            SubscriptionsPatch subscriptionsPatchRequest;

            try
            {
                subscriptionsPatchRequest = await _httpRequestMessageHelper.GetResourceFromRequest<SubscriptionsPatch>(req);
            }
            catch (JsonSerializationException ex)
            {
                _loggerHelper.LogError($"PatchSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions/{subscriptionId} exception {ex.Message}");
                return new UnprocessableEntityObjectResult(_convertToDynamic.ExcludeProperty(ex, ["TargetSite", "InnerException"]));
            }

            if (subscriptionsPatchRequest == null)
            {
                _loggerHelper.LogError($"PatchSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions/{subscriptionId} subscriptionsPatchRequest is null");
                return new UnprocessableEntityObjectResult(req);
            }

            subscriptionsPatchRequest.LastModifiedBy = touchpointId;

            var errors = _validate.ValidateResource(subscriptionsPatchRequest);

            if (errors != null && errors.Any())
            {
                _loggerHelper.LogError($"PatchSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions/{subscriptionId} errors at ValidateResource ");
                return new UnprocessableEntityObjectResult(errors);
            }

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _loggerHelper.LogWarning($"PatchSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions/{subscriptionId} customer doesCustomerExist ");
                return new NoContentResult();
            }

            var subscriptions = await _subscriptionsPatchService.GetSubscriptionsForCustomerAsync(customerGuid, subscriptionsGuid);

            if (subscriptions == null)
            {
                _loggerHelper.LogWarning($"PatchSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions/{subscriptionId} subscriptions  is null ");
                return new NoContentResult();
            }

            var updatedSubscriptions = await _subscriptionsPatchService.UpdateAsync(subscriptions, subscriptionsPatchRequest);
            _loggerHelper.LogInformation($"PatchSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions/{subscriptionId} updatedSubscriptions");


            return updatedSubscriptions == null ?
                new BadRequestObjectResult(subscriptionsGuid) :
                new JsonResult(updatedSubscriptions, new JsonSerializerOptions()) { StatusCode = (int)HttpStatusCode.OK };
        }
    }
}