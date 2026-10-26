using Cloudland.APM.Microservices.Authentication.Services;
using io.fusionauth;
using Cloudland.APM.Utils.ErrorTransform;

namespace Cloudland.APM.Microservices.Authentication.Extensions;

public static class AddTransientExtensions
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddTransient<IJwtService, JWTService>();
        services.AddTransient<IFusionAuthAsyncClient>(_ =>
            new FusionAuthClient(config.GetSection("FusionAuth")["APIKEY"],
                config.GetSection("FusionAuth")["AuthURL"]));
        services.AddTransient<IUserManagementService, UserManagementService>();
        services.AddTransient<ITransformErrorService, TransformErrorService>();
        services.AddHttpContextAccessor();
    }
}