using Cloudland.APM.Microservices.ChatGPT.Services;

namespace Cloudland.APM.Microservices.ChatGPT.Extensions;

public static class AddTransientExtensions
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddTransient<IOpenAiGateway, OpenAiGateway>();
        services.AddHttpContextAccessor();
    }
}