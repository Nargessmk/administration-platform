using Cloudland.APM.Microservices.LocalChatGPT.Services;

namespace Cloudland.APM.Microservices.LocalChatGPT.Extensions;

public static class AddTransientExtensions
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<RavenDbContext>();
        services.AddTransient<IChatService, ChatService>();
        services.AddHttpContextAccessor();
    }
}