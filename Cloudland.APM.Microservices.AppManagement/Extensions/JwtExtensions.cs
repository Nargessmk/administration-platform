using System.Text;
using Cloudland.APM.Models.JWT;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Cloudland.APM.Microservices.AppManagement.Extensions;

public static class JwtExtensions
{
    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>();
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings!.SecretKey!);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        });
        services.AddAuthentication().AddJwtBearer(options =>
        {
            // options.SaveToken = false;
            // options.RequireHttpsMetadata = false;


            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(secretKey)
            };
            
            // This event allows JWT to be read from a cookie if present
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Check for token in "Authorization" header (Bearer token)
                    var accessToken = context.Request.Headers["Authorization"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(accessToken) && accessToken.StartsWith("Bearer "))
                    {
                        context.Token = accessToken["Bearer ".Length..].Trim();
                    }
                    // If no token in header, check for token in a cookie named "jwt_token" (or whatever name you use for the access token cookie)
                    // This is only if you decide to put the access token in an HttpOnly cookie as well.
                    else if (context.Request.Cookies.ContainsKey("jwt_token")) 
                    {
                        context.Token = context.Request.Cookies["jwt_token"];
                    }
                    return Task.CompletedTask;
                }
            };
        });
        services.AddAuthorization(o => { o.AddPolicy("ApiTesterPolicy", b => b.RequireRole("admin")); });
    }
}