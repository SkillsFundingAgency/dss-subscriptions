using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Subscriptions.Annotations;
using NCS.DSS.Subscriptions.Cosmos.Helper;
using NCS.DSS.Subscriptions.GetSubscriptionsForTouchpointHttpTrigger.Service;
using NCS.DSS.Subscriptions.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;

namespace NCS.DSS.Subscriptions.GetSubscriptionsForTouchpointHttpTrigger.Function
{
    public class GetSubscriptionsForTouchpointHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly IGetSubscriptionsForTouchpointHttpTriggerService _getSubscriptionsForTouchpointService;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;

        public GetSubscriptionsForTouchpointHttpTrigger(
            IResourceHelper resourceHelper,
            IHttpRequestHelper httpRequestMessageHelper,
            IGetSubscriptionsForTouchpointHttpTriggerService getSubscriptionsForTouchpointService,
            IHttpResponseMessageHelper httpResponseMessageHelper)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _getSubscriptionsForTouchpointService = getSubscriptionsForTouchpointService;
            _httpResponseMessageHelper = httpResponseMessageHelper;
        }

        [FunctionName("GetByTouchpoint")]
        [ProducesResponseType(typeof(Models.Subscriptions), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Subscriptions found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Subscriptions does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve a single subscriptions with a given SubscriptionsId for an individual customer.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Subscriptions/")] HttpRequest req, ILogger log, string customerId)
        {
            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'APIM-TouchpointId' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }
            
            if (!Guid.TryParse(customerId, out var customerGuid))
                return _httpResponseMessageHelper.BadRequest(customerGuid);
            
            var subscriptions = await _getSubscriptionsForTouchpointService.GetSubscriptionsForTouchpointAsync(customerGuid, touchpointId);

            return subscriptions == null ?
                _httpResponseMessageHelper.NoContent(customerGuid) :
                _httpResponseMessageHelper.Ok(JsonHelper.SerializeObjects(subscriptions));
        }
    }
}