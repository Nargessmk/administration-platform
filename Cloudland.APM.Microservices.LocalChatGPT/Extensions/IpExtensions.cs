using Cloudland.APM.DTO;

namespace Cloudland.APM.Microservices.LocalChatGPT.Extensions;

public class IpApiClient(HttpClient httpClient)
{
    private const string BaseUrl = "https://ip-api.com";

    public async Task<IPApiResponseDTO?> Get(string? ipAddress, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(ipAddress)) return null;

        var route = $"{BaseUrl}/json/{ipAddress}";
        var response = await httpClient.GetFromJsonAsync<IPApiResponseDTO>(route, ct);
        return response;
    }
}