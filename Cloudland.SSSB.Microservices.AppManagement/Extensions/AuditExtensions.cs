using System.Reflection;
using Audit.Core;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace Cloudland.SSSB.Microservices.AppManagement.Extensions;

public static class AuditExtensions
{
    public static void AddAudit(this WebApplicationBuilder builder)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = builder.Configuration;
        var appName = Assembly.GetExecutingAssembly().GetName().Name?.ToLower().Replace(".", "-") ?? "unknown-app";

        var url = configuration["elasticsearch:url"] ?? string.Empty;
        var userName = configuration["elasticsearch:userName"] ?? string.Empty;
        var password = configuration["elasticsearch:password"] ?? string.Empty;

        var client = new ElasticsearchClient(new ElasticsearchClientSettings(new Uri(url))
            .Authentication(new BasicAuthentication(userName, password))
            .RequestTimeout(TimeSpan.FromSeconds(30)));

        Configuration.Setup()
            .UseElasticsearch(config => config
                    .Client(client)
                    .Index(auditEvent => $"{appName}-audit-{environment.ToLower()}-{DateTime.UtcNow:yyyy.MM}")
                    .Id(ev => Guid.NewGuid().ToString())
            )
            .WithCreationPolicy(EventCreationPolicy.InsertOnEnd);
    }
}