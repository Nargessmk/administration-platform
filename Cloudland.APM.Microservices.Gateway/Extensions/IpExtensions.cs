using Cloudland.APM.DTO;

namespace Cloudland.APM.Microservices.Gateway.Extensions;

public sealed class IpApiClient(HttpClient httpClient)
{
    private const string BASE_URL = "http://ip-api.com";

    public async Task<IPApiResponseDTO?> Get(string? ipAddress, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(ipAddress)) return null;

        var route = $"{BASE_URL}/json/{ipAddress}";
        var response = await httpClient.GetFromJsonAsync<IPApiResponseDTO>(route, ct);
        return response;
    }
}