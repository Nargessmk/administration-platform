using System.Reflection;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;

namespace Cloudland.APM.Microservices.AppManagement.Extensions;

public static class SerilogExtensions
{
    public static void AddSerilog(this WebApplicationBuilder builder)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = builder.Configuration;
        var appName = Assembly.GetExecutingAssembly().GetName().Name?.ToLower().Replace(".", "-") ?? "unknown-app";

        SelfLog.Enable(Console.Error);

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Environment", environment)
            .Enrich.WithProperty("Application", appName)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.Logger(lc => lc
                .Filter.ByExcluding(log => log.Level is LogEventLevel.Error or LogEventLevel.Fatal)
                .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment, appName)))
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        builder.Host.UseSerilog();
    }

    private static ElasticsearchSinkOptions ConfigureElasticSink(IConfiguration configuration, string environment, string appName)
    {
        var url = configuration["elasticsearch:url"];
        if (string.IsNullOrEmpty(url))
        {
            SelfLog.WriteLine("--> elasticsearch:url is not configured in appsettings.json. Elasticsearch sink will not be available.");
            return new ElasticsearchSinkOptions() { ConnectionTimeout = TimeSpan.FromMilliseconds(1) };
        }

        string indexFormat = $"{appName}-{environment.ToLower()}-{{0:yyyy.MM}}";

        return new ElasticsearchSinkOptions(new Uri(url))
        {
            AutoRegisterTemplate = true,
            IndexFormat = indexFormat,
            TemplateName = $"{appName}-template",
            NumberOfShards = configuration.GetValue("Elasticsearch:NumberOfShards", 2),
            NumberOfReplicas = configuration.GetValue("Elasticsearch:NumberOfReplicas", 1),
            ModifyConnectionSettings = conn => conn
                .BasicAuthentication(
                    configuration["elasticsearch:userName"],
                    configuration["elasticsearch:password"])
                .EnableApiVersioningHeader()
                .RequestTimeout(TimeSpan.FromSeconds(30)),
            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv8,
            TypeName = null,
            BatchAction = ElasticOpType.Create,
            CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true),
            RegisterTemplateFailure = RegisterTemplateRecovery.IndexAnyway,
            EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog | EmitEventFailureHandling.RaiseCallback
        };
    }
}