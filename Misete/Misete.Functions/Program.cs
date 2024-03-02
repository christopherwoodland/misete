var host = new HostBuilder()
.ConfigureAppConfiguration(configureDelegate: (hostingContext, config) =>
{
    var settings = config.Build();
    config.AddAzureAppConfiguration(options =>
    {
        options.Connect(Environment.GetEnvironmentVariable("AZURE_APP_CONFIG"))
        .Select("OPENAI:*")
        .Select("COSMOSDB:*")
        .Select("COGSERVICE:*")
        .Select("DOCINTELL:*")
        .Select("CUSTOMVISION:*")
        .Select("AZUREMAPS:*")
        .Select("MISETE:*")
        .ConfigureRefresh(refreshOptions => refreshOptions.Register("OPENAI:OAI_EMBEDDINGS_DEPLOYMENT_NAME", refreshAll: true))
        .ConfigureRefresh(refreshOptions => refreshOptions.Register("OPENAI:OAI_END_POINT", refreshAll: true))
        .ConfigureRefresh(refreshOptions => refreshOptions.Register("OPENAI:OAI_KEY", refreshAll: true))
        .ConfigureRefresh(refreshOptions => refreshOptions.Register("OPENAI:MAX_TOKENS", refreshAll: true));
    });
})
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(s =>
    {
        s.AddApplicationInsightsTelemetryWorkerService();
        s.ConfigureFunctionsApplicationInsights();
        s.AddAzureAppConfiguration();
        s.AddScoped<IAppConfigurationHelper, AppConfigurationHelper>();
        s.AddScoped<IImageAnalysisHelper, ImageAnalysisHelper>();
        s.AddScoped<IMapsHelper, MapsHelper>();
        s.AddScoped<IBlobHelper, BlobHelper>();
        s.Configure<LoggerFilterOptions>(options =>
        {
            // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
            // Log levels can also be configured using appsettings.json. For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
            LoggerFilterRule? toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

            if (toRemove is not null)
            {
                options.Rules.Remove(toRemove);
            }
        });
    })
    .Build();
host.Run();
