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
            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogInformation($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions Unable to locate 'TouchpointId' in request header");
                return new BadRequestResult();
            }

            _logger.LogInformation("C# HTTP trigger function processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogInformation($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions customerGuid BadRequest");
                return new BadRequestObjectResult(customerGuid);
            }

            Models.Subscriptions subscriptionsRequest;

            try
            {
                subscriptionsRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Subscriptions>(req);
            }
            catch (Exception ex)
            {
                _logger.LogError($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions JsonSerializationException");
                return new UnprocessableEntityObjectResult(_convertToDynamic.ExcludeProperty(ex, ["TargetSite", "InnerException"]));
            }

            if (subscriptionsRequest == null)
            {
                _logger.LogError($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions subscriptionsRequest is null");
                return new UnprocessableEntityObjectResult(req);
            }

            subscriptionsRequest.SetIds(customerGuid, touchpointId);

            var errors = _validate.ValidateResource(subscriptionsRequest);

            if (errors != null && errors.Count > 0)
            {
                _logger.LogError($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions errors in ValidateResource");
                return new UnprocessableEntityObjectResult(errors);
            }

            var doesCustomerExist = await _subscriptionsPostService.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogError($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions does notvCustomerExist ");
                return new NoContentResult();
            }

            var doesSubscriptionExist = await _subscriptionsPostService.DoesSubscriptionExist(customerGuid, touchpointId);

            if (doesSubscriptionExist.HasValue)
            {
                var duplicateError = _validate.ValidateResultForDuplicateSubscriptionId(doesSubscriptionExist.GetValueOrDefault());
                _logger.LogError($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions Subscription conflict. {duplicateError} ");
                return new ConflictResult();
            }

            var subscriptions = await _subscriptionsPostService.CreateAsync(subscriptionsRequest);

            _logger.LogInformation($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions CreateAsync called");

            return subscriptions == null
                ? new BadRequestObjectResult(customerGuid)
                : new JsonResult(subscriptions, new JsonSerializerOptions()) { StatusCode = (int)HttpStatusCode.Created };
        }
    }
}