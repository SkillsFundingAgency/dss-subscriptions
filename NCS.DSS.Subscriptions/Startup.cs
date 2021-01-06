using DFC.HTTP.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Subscriptions;
using NCS.DSS.Subscriptions.Cosmos.Helper;
using NCS.DSS.Subscriptions.GetSubscriptionsByIdHttpTrigger.Service;
using NCS.DSS.Subscriptions.GetSubscriptionsForTouchpointHttpTrigger.Service;
using NCS.DSS.Subscriptions.Helpers;
using NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Service;
using NCS.DSS.Subscriptions.PostSubscriptionsHttpTrigger.Service;
using NCS.DSS.Subscriptions.Validation;

[assembly: FunctionsStartup(typeof(Startup))]
namespace NCS.DSS.Subscriptions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IGetSubscriptionsForTouchpointHttpTriggerService, GetSubscriptionsForTouchpointHttpTriggerService>();
            builder.Services.AddTransient<IGetSubscriptionsByIdHttpTriggerService, GetSubscriptionsByIdHttpTriggerService>();
            builder.Services.AddTransient<IPostSubscriptionsHttpTriggerService, PostSubscriptionsHttpTriggerService>();
            builder.Services.AddTransient<IPatchSubscriptionsHttpTriggerService, PatchSubscriptionsHttpTriggerService>();


            builder.Services.AddTransient<IResourceHelper, ResourceHelper>();
            builder.Services.AddTransient<IValidate, Validate>();
            builder.Services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
            builder.Services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
            builder.Services.AddTransient<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
        }
    }
}
