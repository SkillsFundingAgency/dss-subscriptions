using System;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Subscriptions.Cosmos.Helper;
using NCS.DSS.Subscriptions.Helpers;
using NCS.DSS.Subscriptions.GetSubscriptionsByIdHttpTrigger.Service;
using NCS.DSS.Subscriptions.GetSubscriptionsForTouchpointHttpTrigger.Service;
using NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Service;
using NCS.DSS.Subscriptions.PostSubscriptionsHttpTrigger;
using NCS.DSS.Subscriptions.PostSubscriptionsHttpTrigger.Service;
using NCS.DSS.Subscriptions.Validation;

namespace NCS.DSS.Subscriptions.Ioc
{
    public class RegisterServiceProvider
    {
        public IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddTransient<IGetSubscriptionsForTouchpointHttpTriggerService, GetSubscriptionsForTouchpointHttpTriggerService>();
            services.AddTransient<IGetSubscriptionsByIdHttpTriggerService, GetSubscriptionsByIdHttpTriggerService>();
            services.AddTransient<IPostSubscriptionsHttpTriggerService, PostSubscriptionsHttpTriggerService>();
            services.AddTransient<IPatchSubscriptionsHttpTriggerService, PatchSubscriptionsHttpTriggerService>();
            

            services.AddTransient<IResourceHelper, ResourceHelper>();
            services.AddTransient<IValidate, Validate>();
            services.AddTransient<IHttpRequestMessageHelper, HttpRequestMessageHelper>();
            return services.BuildServiceProvider(true);
        }
    }
}
