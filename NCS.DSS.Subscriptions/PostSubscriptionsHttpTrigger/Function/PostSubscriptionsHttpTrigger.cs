using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Subscriptions.Helpers;
using NCS.DSS.Subscriptions.PostSubscriptionsHttpTrigger.Service;
using NCS.DSS.Subscriptions.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Subscriptions.PostSubscriptionsHttpTrigger.Function
{
    public class PostSubscriptionsHttpTrigger
    {
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IValidate _validate;
        private readonly IPostSubscriptionsHttpTriggerService _subscriptionsPostService;
        private readonly ILogger<PostSubscriptionsHttpTrigger> _logger;
        private readonly IConvertToDynamic _convertToDynamic;

        public PostSubscriptionsHttpTrigger(
            IHttpRequestHelper httpRequestMessageHelper,
            IValidate validate,
            IPostSubscriptionsHttpTriggerService subscriptionsPostService,
            ILogger<PostSubscriptionsHttpTrigger> logger,
            IConvertToDynamic convertToDynamic)
        {
            _httpRequestHelper = httpRequestMessageHelper;
            _validate = validate;
            _subscriptionsPostService = subscriptionsPostService;
            _logger = logger;
            _convertToDynamic = convertToDynamic;
        }

        [Function("Post")]
        [ProducesResponseType(typeof(Models.Subscriptions), (int)HttpStatusCode.Created)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Subscriptions Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Subscriptions does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Subscriptions validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new subscriptions for a given customer")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Subscriptions")] HttpRequest req, string customerId)
        {
            var functionName = nameof(PostSubscriptionsHttpTrigger);

            _logger.LogTrace("Function {FunctionName} has been invoked", functionName);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogTrace("Unable to parse 'DssCorrelationId' to a Guid. New Guid Generated.");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogInformation($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions Unable to locate 'TouchpointId' in request header");
                return new BadRequestResult();
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogInformation($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions customerGuid BadRequest");
                return new BadRequestObjectResult(customerGuid);
            }
            _logger.LogTrace("{CorrelationId} Input validation has succeeded.", correlationId);

            Models.Subscriptions subscriptionsRequest;

            try
            {
                _logger.LogTrace("{CorrelationId} Attempt to get resource from body of the request", correlationId);

                subscriptionsRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Subscriptions>(req);
            }
            catch (Exception ex)
            {
                var response = new UnprocessableEntityObjectResult(_convertToDynamic.ExcludeProperty(ex, ["TargetSite", "InnerException"]));
                _logger.LogError(ex, "{CorrelationId} Response Status Code: {StatusCode}. Unable to retrieve body from req", correlationId, response.StatusCode);
                return response;
            }

            if (subscriptionsRequest == null)
            {
                var response = new UnprocessableEntityObjectResult(req);
                _logger.LogInformation("{CorrelationId} Response Status Code: {StatusCode}. session patch request is null", correlationId, response.StatusCode);
                return response;
            }
            _logger.LogTrace("{CorrelationId} Attempt to set id's for subscription post", correlationId);
            subscriptionsRequest.SetIds(customerGuid, touchpointId);
            _logger.LogTrace("{CorrelationId} Attempt to validate resource", correlationId);
            var errors = _validate.ValidateResource(subscriptionsRequest);

            if (errors != null && errors.Count > 0)
            {
                var response = new UnprocessableEntityObjectResult(string.Join(',', errors));
                _logger.LogInformation("{CorrelationId} Response Status Code: {StatusCode}. validation errors with resource {Errors}", correlationId, response.StatusCode,errors);
                return response;
            }

            _logger.LogTrace("{CorrelationId} Attempting to see if customer exists {customerGuid}", correlationId, customerGuid);

            var doesCustomerExist = await _subscriptionsPostService.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response = new NoContentResult();
                _logger.LogInformation("{CorrelationId} Response Status Code: {StatusCode}. Customer does not exist {CustomerId}", correlationId, customerGuid);
                return response;
            }
            _logger.LogTrace("{CorrelationId} Attempting to see if Subscription already exists for {customerGuid} and touchpoint with {touchpointId}", correlationId, customerGuid,touchpointId );

            var doesSubscriptionExist = await _subscriptionsPostService.DoesSubscriptionExist(customerGuid, touchpointId);

            if (doesSubscriptionExist.HasValue)
            {
                var duplicateError = _validate.ValidateResultForDuplicateSubscriptionId(doesSubscriptionExist.GetValueOrDefault());
                var response = new ConflictResult();
                _logger.LogInformation("{CorrelationId} Response Status Code: {StatusCode}. Subscriptions exists for customer {customerGuid} and touchpoint with {touchpointId}. Error {Error}", correlationId, response.StatusCode, customerGuid, touchpointId, duplicateError.ErrorMessage);
                return response;
            }

            _logger.LogTrace("{CorrelationId} Attempting to Create Subscription for Customer {customerGuid}", correlationId, customerGuid);

            var subscriptions = await _subscriptionsPostService.CreateAsync(subscriptionsRequest);

            if (subscriptions == null)
            {
                var response = new BadRequestObjectResult(customerGuid);
                _logger.LogInformation("{CorrelationId} Response Status Code: {StatusCode}. Failed to Create the Subscription for Customer {customerGuid}", correlationId, response.StatusCode, customerGuid);
                _logger.LogTrace("Function {FunctionName} has finished invoking", functionName);
                return response;
            }
            else
            {
                var response = new JsonResult(subscriptions, new JsonSerializerOptions()) { StatusCode = (int)HttpStatusCode.Created };
                _logger.LogTrace("{CorrelationId} Response Status Code: {StatusCode}. Successfully patched the Subscription for Customer {customerGuid}", correlationId, response.StatusCode, customerGuid);
                _logger.LogTrace("Function {FunctionName} has finished invoking", functionName);
                return response;
            }
        }
    }
}