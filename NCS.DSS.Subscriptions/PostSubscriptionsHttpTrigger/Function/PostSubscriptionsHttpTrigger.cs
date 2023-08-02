using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Subscriptions.Cosmos.Helper;
using NCS.DSS.Subscriptions.Helpers;
using NCS.DSS.Subscriptions.PostSubscriptionsHttpTrigger.Service;
using NCS.DSS.Subscriptions.Validation;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Subscriptions.PostSubscriptionsHttpTrigger.Function
{
    public class PostSubscriptionsHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IValidate _validate;
        private readonly IPostSubscriptionsHttpTriggerService _subscriptionsPostService;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;

        public PostSubscriptionsHttpTrigger(
            IResourceHelper resourceHelper,
            IHttpRequestHelper httpRequestMessageHelper,
            IValidate validate,
            IPostSubscriptionsHttpTriggerService subscriptionsPostService,
            IHttpResponseMessageHelper httpResponseMessageHelper)
        {
            _resourceHelper = resourceHelper;
            _httpRequestHelper = httpRequestMessageHelper;
            _validate = validate;
            _subscriptionsPostService = subscriptionsPostService;
            _httpResponseMessageHelper = httpResponseMessageHelper;
        }

        [FunctionName("Post")]
        [ProducesResponseType(typeof(Models.Subscriptions), (int)HttpStatusCode.Created)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Subscriptions Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Subscriptions does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Subscriptions validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new subscriptions for a given customer")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Subscriptions")] HttpRequest req, ILogger log, string customerId)
        {
            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions Unable to locate 'TouchpointId' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("C# HTTP trigger function processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                log.LogInformation($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions customerGuid BadRequest");
                return _httpResponseMessageHelper.BadRequest(customerGuid);
            }

            Models.Subscriptions subscriptionsRequest;

            try
            {
                subscriptionsRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Subscriptions>(req);
            }
            catch (JsonSerializationException ex)
            {
                log.LogError($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions JsonSerializationException");
                return _httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (subscriptionsRequest == null)
            {
                log.LogError($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions subscriptionsRequest is null");
                return _httpResponseMessageHelper.UnprocessableEntity(req);
            }

            subscriptionsRequest.SetIds(customerGuid, touchpointId);

            var errors = _validate.ValidateResource(subscriptionsRequest);

            if (errors != null && errors.Any())
            {
                log.LogError($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions errors in ValidateResource");
                return _httpResponseMessageHelper.UnprocessableEntity(errors);
            }

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                log.LogError($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions does notvCustomerExist ");
                return _httpResponseMessageHelper.NoContent(customerGuid);
            }

            var doesSubscriptionExist = await _resourceHelper.DoesSubscriptionExist(customerGuid, touchpointId);

            if (doesSubscriptionExist.HasValue)
            {
                var duplicateError = _validate.ValidateResultForDuplicateSubscriptionId(doesSubscriptionExist.GetValueOrDefault());
                log.LogError($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions Subscription conflict ");
                return _httpResponseMessageHelper.Conflict();
            }

            var subscriptions = await _subscriptionsPostService.CreateAsync(subscriptionsRequest);

            log.LogInformation($"PostSubscriptionsHttpTrigger Customers/{customerId}/Subscriptions CreateAsync called");

            return subscriptions == null
                ? _httpResponseMessageHelper.BadRequest(customerGuid)
                : _httpResponseMessageHelper.Created(JsonHelper.SerializeObject(subscriptions));
        }
    }
}