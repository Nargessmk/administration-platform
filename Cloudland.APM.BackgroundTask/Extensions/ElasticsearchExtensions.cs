using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Cloudland.APM.DTO;

namespace Cloudland.APM.BackgroundTask.Extensions;

public static class ElasticsearchExtensions
{
    public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        var url = configuration["elasticsearch:url"];
        var defaultIndex = configuration["elasticsearch:index"] ?? "sin";

        if (url == null) return;

        var settings = new ElasticsearchClientSettings(new Uri(url))
            .DefaultIndex(defaultIndex)
            .EnableDebugMode()
            .EnableHttpCompression()
            .ServerCertificateValidationCallback((_, _, _, _) => true);

        // Add basic authentication
        var username = configuration["elasticsearch:userName"];
        var password = configuration["elasticsearch:password"];
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            settings.Authentication(new BasicAuthentication(username, password));

        // Create the client
        var client = new ElasticsearchClient(settings);

        // Register mappings asynchronously
        Task.Run(() => RegisterMappingsAsync(client)).Wait();

        // Register as singleton
        services.AddSingleton(client);
    }

    private static async Task RegisterMappingsAsync(ElasticsearchClient client)
    {
        // Create indices if they don't exist and define mappings
        var indices = new[]
        {
            nameof(IPApiResponseDTO).ToLower()
        };

        foreach (var index in indices)
        {
            var existsResponse = await client.Indices.ExistsAsync(index);
            if (!existsResponse.Exists) await client.Indices.CreateAsync(index);
        }
    }
}