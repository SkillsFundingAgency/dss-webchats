using Azure.Identity;
using Azure.Messaging.ServiceBus;
using DFC.HTTP.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.WebChat.Cosmos.Provider;
using NCS.DSS.WebChat.Helpers;
using NCS.DSS.WebChat.Models;
using NCS.DSS.WebChat.PatchWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.PostWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.ServiceBus;
using NCS.DSS.WebChat.Validation;
namespace NCS.DSS.WebChat
{
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
                    services.AddOptions<WebChatConfigurationSettings>()
                    .Bind(configuration);
                    services.AddLogging();
                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();
                    services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
                    services.AddTransient<IPostWebChatHttpTriggerService, PostWebChatHttpTriggerService>();
                    services.AddTransient<IPatchWebChatHttpTriggerService, PatchWebChatHttpTriggerService>();
                    services.AddTransient<IValidate, Validate>();
                    services.AddTransient<IHttpRequestHelper, HttpRequestHelper>();
                    services.AddTransient<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
                    services.AddSingleton<IDynamicHelper, DynamicHelper>();
                    services.AddSingleton<ICosmosDBProvider, CosmosDBProvider>();
                    services.Configure<LoggerFilterOptions>(options =>
                    {
                        LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                            == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                        if (toRemove is not null)
                        {
                            options.Rules.Remove(toRemove);
                        }
                    });
                    services.AddSingleton(sp =>
                    {
                        var cosmosDbEndpoint = configuration["CosmosDbEndpoint"];
                        if (string.IsNullOrEmpty(cosmosDbEndpoint))
                        {
                            throw new InvalidOperationException("CosmosDbEndpoint is not configured.");
                        }

                        var options = new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway };
                        return new CosmosClient(cosmosDbEndpoint, new DefaultAzureCredential(), options);
                    });
                    services.AddScoped<IWebChatServiceBusClient, WebChatServiceBusClient>();
                    services.AddSingleton(serviceProvider =>
                    {
                        var settings = serviceProvider.GetRequiredService<IOptions<WebChatConfigurationSettings>>().Value;
                        string connectionString = $"Endpoint={settings.BaseAddress};SharedAccessKeyName={settings.KeyName};SharedAccessKey={settings.AccessKey}";
                        return new ServiceBusClient(connectionString);
                    });
                })
                .Build();

            await host.RunAsync();
        }
    }
}

