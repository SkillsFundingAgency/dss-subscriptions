using DFC.HTTP.Standard;
using DFC.Swagger.Standard;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCS.DSS.Subscriptions.Cosmos.Helper;
using NCS.DSS.Subscriptions.GetSubscriptionsByIdHttpTrigger.Service;
using NCS.DSS.Subscriptions.GetSubscriptionsForTouchpointHttpTrigger.Service;
using NCS.DSS.Subscriptions.Helpers;
using NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Service;
using NCS.DSS.Subscriptions.PostSubscriptionsHttpTrigger.Service;
using NCS.DSS.Subscriptions.Validation;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices( services =>
    {
        services.AddTransient<IGetSubscriptionsForTouchpointHttpTriggerService, GetSubscriptionsForTouchpointHttpTriggerService>();
        services.AddTransient<IGetSubscriptionsByIdHttpTriggerService, GetSubscriptionsByIdHttpTriggerService>();
        services.AddTransient<IPostSubscriptionsHttpTriggerService, PostSubscriptionsHttpTriggerService>();
        services.AddTransient<IPatchSubscriptionsHttpTriggerService, PatchSubscriptionsHttpTriggerService>();
        services.AddSingleton<IConvertToDynamic, ConvertToDynamic>();
        services.AddTransient<IResourceHelper, ResourceHelper>();
        services.AddTransient<IValidate, Validate>();
        services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
        services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
        services.AddTransient<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
    })
    .Build();

host.Run();
