using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Subscriptions.GetSubscriptionsForTouchpointHttpTrigger.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace NCS.DSS.Subscriptions.GetSubscriptionsForTouchpointHttpTrigger.Function
{
    public class GetSubscriptionsForTouchpointHttpTrigger
    {
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly IGetSubscriptionsForTouchpointHttpTriggerService _getSubscriptionsForTouchpointService;
        private readonly ILogger<GetSubscriptionsForTouchpointHttpTrigger> _logger;

        public GetSubscriptionsForTouchpointHttpTrigger(
            IHttpRequestHelper httpRequestMessageHelper,
            IGetSubscriptionsForTouchpointHttpTriggerService getSubscriptionsForTouchpointService,
            ILogger<GetSubscriptionsForTouchpointHttpTrigger> logger)
        {
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _getSubscriptionsForTouchpointService = getSubscriptionsForTouchpointService;
            _logger = logger;
        }

        [Function("GetByTouchpoint")]
        [ProducesResponseType(typeof(Models.Subscriptions), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Subscriptions found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Subscriptions does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve a single subscriptions with a given SubscriptionsId for an individual customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Subscriptions/")] HttpRequest req, string customerId)
        {
            var functionName = nameof(GetSubscriptionsForTouchpointHttpTrigger);

            _logger.LogTrace("Function {FunctionName} has been invoked", functionName);

            var correlationId = _httpRequestMessageHelper.GetDssCorrelationId(req);

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid. New Guid Generated.");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                var response = new BadRequestObjectResult(HttpStatusCode.BadRequest);
                _logger.LogInformation("{CorrelationId} Response Status Code: {StatusCode}. Unable to locate 'TouchpointId' in request header", correlationId, response.StatusCode);
                return response;
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                var response = new BadRequestObjectResult(customerGuid);
                _logger.LogInformation("{CorrelationId} Response Status Code: {StatusCode}. Unable to parse 'customerId' to a Guid: {customerId}", correlationId, customerId);
                return response;
            }

            _logger.LogTrace("{CorrelationId} Input validation has succeeded.", correlationId);

            _logger.LogTrace("{CorrelationId} Attempting to see if customer exists {customerGuid}", correlationId, customerGuid);
            var doesCustomerExist = await _getSubscriptionsForTouchpointService.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response = new NoContentResult();
                _logger.LogInformation("{CorrelationId} Response Status Code: {StatusCode}. Customer does not exist {customerGuid}", correlationId, response.StatusCode, customerGuid);
                return response;
            }
            _logger.LogTrace("{CorrelationId} Customer record found in Cosmos DB {customerGuid}", correlationId, customerGuid);

            _logger.LogTrace("{CorrelationId} Attempting to get Subscriptions for customer {customerGuid} and {touchpointId}", correlationId, customerGuid, touchpointId);
            var subscriptions = await _getSubscriptionsForTouchpointService.GetSubscriptionsForTouchpointAsync(customerGuid, touchpointId);

            if (subscriptions == null)
            {
                var response = new NoContentResult();
                _logger.LogInformation("{CorrelationId} Response Status Code: {StatusCode}. Subscriptions does not exist for customer {customerGuid} and {touchpointId}", correlationId, response.StatusCode, customerGuid, touchpointId);
                _logger.LogTrace("Function {FunctionName} has finished invoking", functionName);
                return response;
            }

            var jsonResponse = new JsonResult(subscriptions[0], new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
            _logger.LogTrace("{CorrelationId} Response Status Code: {StatusCode}. Get session succeeded for customer {customerGuid} and {touchpointId}", correlationId, jsonResponse.StatusCode, customerGuid, touchpointId);
            _logger.LogTrace("Function {FunctionName} has finished invoking", functionName);
            if (subscriptions.Count > 1)            
            {               
                jsonResponse.Value = subscriptions;
            }
            return jsonResponse;
        }
    }
}