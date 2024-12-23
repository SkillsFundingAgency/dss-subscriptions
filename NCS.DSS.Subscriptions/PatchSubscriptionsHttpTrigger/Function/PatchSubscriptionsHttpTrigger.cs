using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Subscriptions.Helpers;
using NCS.DSS.Subscriptions.Models;
using NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Service;
using NCS.DSS.Subscriptions.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Function
{
    public class PatchSubscriptionsHttpTrigger
    {
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly IValidate _validate;
        private readonly IPatchSubscriptionsHttpTriggerService _subscriptionsPatchService;
        private readonly ILogger<PatchSubscriptionsHttpTrigger> _logger;
        private readonly IConvertToDynamic _convertToDynamic;
        public PatchSubscriptionsHttpTrigger(
           IHttpRequestHelper httpRequestMessageHelper,
           IValidate validate,
           IPatchSubscriptionsHttpTriggerService subscriptionsPatchService,
           IConvertToDynamic convertToDynamic,
           ILogger<PatchSubscriptionsHttpTrigger> logger)
        {
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _validate = validate;
            _subscriptionsPatchService = subscriptionsPatchService;
            _convertToDynamic = convertToDynamic;
            _logger = logger;
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
            var functionName = nameof(PatchSubscriptionsHttpTrigger);

            _logger.LogInformation("Function {FunctionName} has been invoked", functionName);

            var correlationId = _httpRequestMessageHelper.GetDssCorrelationId(req);

            if (string.IsNullOrEmpty(correlationId))
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid. New Guid Generated.");
                correlationGuid = Guid.NewGuid();
            }
            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                var response = new BadRequestObjectResult(HttpStatusCode.BadRequest);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to locate 'TouchpointId' in request header", correlationId, response.StatusCode);
                return response;
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                var response = new BadRequestObjectResult(customerGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to parse 'customerId' to a Guid: {customerId}", correlationId, response.StatusCode, customerId);
                return response;
            }

            if (!Guid.TryParse(subscriptionId, out var subscriptionsGuid))
            {
                var response = new BadRequestObjectResult(subscriptionId);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to parse 'subscriptionId' to a Guid: {subscriptionId}", correlationId, response.StatusCode, subscriptionId);
                return response;
            }
            _logger.LogInformation("{CorrelationId} Input validation has succeeded.", correlationId);
            SubscriptionsPatch subscriptionsPatchRequest;

            try
            {
                _logger.LogInformation("{CorrelationId} Attempt to get resource from body of the request", correlationId);
                subscriptionsPatchRequest = await _httpRequestMessageHelper.GetResourceFromRequest<SubscriptionsPatch>(req);
            }
            catch (Exception ex)
            {
                var response = new UnprocessableEntityObjectResult(_convertToDynamic.ExcludeProperty(ex, ["TargetSite", "InnerException"]));
                _logger.LogError(ex, "{CorrelationId} Response Status Code: {StatusCode}. Unable to retrieve body from req", correlationId, response.StatusCode);
                return response;               
            }

            if (subscriptionsPatchRequest == null)
            {
                var response = new UnprocessableEntityObjectResult(req);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. session patch request is null", correlationId, response.StatusCode);
                return response;
            }

            subscriptionsPatchRequest.LastModifiedBy = touchpointId;
            _logger.LogInformation("{CorrelationId} Attempt to validate resource", correlationId);
            var errors = _validate.ValidateResource(subscriptionsPatchRequest);

            if (errors != null && errors.Any())
            {
                var response = new UnprocessableEntityObjectResult(string.Join(',', errors));
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. validation errors with resource {Errors}", correlationId, response.StatusCode,errors);
                return response;
            }
            _logger.LogInformation("{CorrelationId} Attempting to see if customer exists {customerGuid}", correlationId, customerGuid);

            var doesCustomerExist = await _subscriptionsPatchService.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response = new NoContentResult();
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Customer does not exist {CustomerId}", correlationId, customerGuid);
                return response;
            }
            _logger.LogInformation("{CorrelationId} Customer record found in Cosmos DB {customerGuid}", correlationId, customerGuid);
            
            _logger.LogInformation("{CorrelationId} Attempting to get Subscriptions for customer {customerGuid} and subscription with {subscriptionsGuid}", correlationId, customerGuid, subscriptionsGuid);

            var subscriptions = await _subscriptionsPatchService.GetSubscriptionsForCustomerAsync(customerGuid, subscriptionsGuid);

            if (subscriptions == null)
            {
                var response = new NoContentResult();
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Subscriptions does not exist for customer {customerGuid} and subscription with {subscriptionsGuid}", correlationId, response.StatusCode, customerGuid, subscriptionsGuid);                
                return response;
            }

            _logger.LogInformation("{CorrelationId} Attempting to Patch Subscription with ID {subscriptionsGuid}", correlationId, subscriptionsGuid);
            var updatedSubscriptions = await _subscriptionsPatchService.UpdateAsync(subscriptions, subscriptionsPatchRequest);

            if (updatedSubscriptions == null)
            {
                var response = new BadRequestObjectResult(subscriptionsGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Failed to patch the Subscription with ID {subscriptionsGuid}", correlationId, response.StatusCode, subscriptionsGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return response;
            }
            else
            {
                var response = new JsonResult(updatedSubscriptions, new JsonSerializerOptions()) { StatusCode = (int)HttpStatusCode.OK };
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Successfully patched the Subscription with ID {subscriptionsGuid}", correlationId, response.StatusCode, subscriptionsGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return response;
            }   
        }
    }
}