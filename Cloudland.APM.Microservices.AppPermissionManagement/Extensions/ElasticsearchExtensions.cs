using Cloudland.APM.DTO;
using Nest;

namespace Cloudland.APM.Microservices.AppPermissionManagement.Extensions;

public static class ElasticsearchExtensions
{
    public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        var url = configuration["elasticsearch:url"];
        var defaultIndex = configuration["elasticsearch:index"];

        if (url == null) return;
        var settings = new ConnectionSettings(new Uri(url))
            .DefaultIndex(defaultIndex)
            .PrettyJson()
            .EnableApiVersioningHeader()
            .EnableHttpCompression()
            .ServerCertificateValidationCallback((o, certificate, chain, errors) => true);
        settings.BasicAuthentication(configuration["elasticsearch:userName"], configuration["elasticsearch:password"]);
        AddDefaultMappings(settings);
        var client = new ElasticClient(settings);
        services.AddSingleton<IElasticClient>(client);
    }

    private static void AddDefaultMappings(ConnectionSettings settings)
    {
        settings.DefaultMappingFor<ConstantDTO>(m => m
            .IndexName(nameof(ConstantDTO).ToLower())
            .IdProperty(p => p.Id));
    }
}