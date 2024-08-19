using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Subscriptions.Cosmos.Helper;
using NCS.DSS.Subscriptions.GetSubscriptionsForTouchpointHttpTrigger.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Subscriptions.GetSubscriptionsForTouchpointHttpTrigger.Function
{
    public class GetSubscriptionsForTouchpointHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly IGetSubscriptionsForTouchpointHttpTriggerService _getSubscriptionsForTouchpointService;
        private readonly ILogger<GetSubscriptionsForTouchpointHttpTrigger> _loggerHelper;

        public GetSubscriptionsForTouchpointHttpTrigger(
            IResourceHelper resourceHelper,
            IHttpRequestHelper httpRequestMessageHelper,
            IGetSubscriptionsForTouchpointHttpTriggerService getSubscriptionsForTouchpointService,
            ILogger<GetSubscriptionsForTouchpointHttpTrigger> loggerHelper)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _getSubscriptionsForTouchpointService = getSubscriptionsForTouchpointService;
            _loggerHelper = loggerHelper;
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
                _loggerHelper.LogInformation("Unable to locate 'APIM-TouchpointId' in request header");
                return new BadRequestResult();
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _loggerHelper.LogWarning($"GetSubscriptionsForTouchpointHttpTrigger Customers/{customerId}/Subscriptions/ BadRequest");
                return new BadRequestObjectResult(customerGuid);
            }

            var subscriptions = await _getSubscriptionsForTouchpointService.GetSubscriptionsForTouchpointAsync(customerGuid, touchpointId);
            _loggerHelper.LogInformation($"GetSubscriptionsForTouchpointHttpTrigger Customers/{customerId}/Subscriptions");

            return subscriptions == null ?
                new NoContentResult() :
                new JsonResult(subscriptions, new JsonSerializerOptions()) { StatusCode = (int)HttpStatusCode.OK };
        }
    }
}