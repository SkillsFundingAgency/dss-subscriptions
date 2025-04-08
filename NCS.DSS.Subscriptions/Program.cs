using DFC.HTTP.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Subscriptions.Cosmos.Provider;
using NCS.DSS.Subscriptions.GetSubscriptionsForTouchpointHttpTrigger.Service;
using NCS.DSS.Subscriptions.Helpers;
using NCS.DSS.Subscriptions.Models;
using NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Service;
using NCS.DSS.Subscriptions.PostSubscriptionsHttpTrigger.Service;
using NCS.DSS.Subscriptions.Validation;
internal class Program
{
    private static async Task Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWebApplication()
            .ConfigureAppConfiguration(configBuilder =>
            {
                configBuilder.SetBasePath(Environment.CurrentDirectory)
                    .AddJsonFile("local.settings.json", optional: true,
                        reloadOnChange: false)
                    .AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;
                services.AddOptions<SubscriptionsConfigurationSettings>()
                    .Bind(configuration);
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();
                services.AddLogging();
                services.AddTransient<IGetSubscriptionsForTouchpointHttpTriggerService, GetSubscriptionsForTouchpointHttpTriggerService>();
                services.AddTransient<IPostSubscriptionsHttpTriggerService, PostSubscriptionsHttpTriggerService>();
                services.AddTransient<IPatchSubscriptionsHttpTriggerService, PatchSubscriptionsHttpTriggerService>();
                services.AddSingleton<IConvertToDynamic, ConvertToDynamic>();
                services.AddSingleton<ICosmosDBProvider, CosmosDBProvider>();
                services.AddSingleton(sp =>
                {
                    var settings = sp.GetRequiredService<IOptions<SubscriptionsConfigurationSettings>>().Value;
                    var options = new CosmosClientOptions()
                    {
                        ConnectionMode = ConnectionMode.Gateway
                    };
                    return new CosmosClient(settings.SubscriptionsConnectionString, options);
                });
                services.AddTransient<IValidate, Validate>();
                services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
                services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
                services.AddTransient<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
                services.Configure<LoggerFilterOptions>(options =>
                {
                    LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                        == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                    if (toRemove is not null)
                    {
                        options.Rules.Remove(toRemove);
                    }
                });

            })
            .Build();

        await host.RunAsync();
    }
}