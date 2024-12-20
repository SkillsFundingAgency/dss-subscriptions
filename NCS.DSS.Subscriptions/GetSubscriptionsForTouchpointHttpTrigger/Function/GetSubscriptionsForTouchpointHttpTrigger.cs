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
            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogInformation("Unable to locate 'APIM-TouchpointId' in request header");
                return new BadRequestResult();
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning($"GetSubscriptionsForTouchpointHttpTrigger Customers/{customerId}/Subscriptions/ BadRequest");
                return new BadRequestObjectResult(customerGuid);
            }
            var doesCustomerExist = await _getSubscriptionsForTouchpointService.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogError($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions does notvCustomerExist ");
                return new NoContentResult();
            }
            var subscriptions = await _getSubscriptionsForTouchpointService.GetSubscriptionsForTouchpointAsync(customerGuid, touchpointId);
            _logger.LogInformation($"GetSubscriptionsForTouchpointHttpTrigger Customers/{customerId}/Subscriptions");

            if (subscriptions == null)
            {
                _logger.LogWarning($"Subscriptions not found for customer id [{customerId}]");
                return new NoContentResult();
            }
            else if (subscriptions.Count == 1)
                return new JsonResult(subscriptions[0], new JsonSerializerOptions()) { StatusCode = (int)HttpStatusCode.OK };

            return new JsonResult(subscriptions, new JsonSerializerOptions()) { StatusCode = (int)HttpStatusCode.OK };
        }
    }
}