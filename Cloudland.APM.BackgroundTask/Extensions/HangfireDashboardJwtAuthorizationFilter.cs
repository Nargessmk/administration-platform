using System.IdentityModel.Tokens.Jwt;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Cloudland.APM.BackgroundTask.Extensions;

public class HangfireDashboardJwtAuthorizationFilter(
    TokenValidationParameters tokenValidationParameters,
    string role = null!)
    : IDashboardAuthorizationFilter
{
    private const string HangFireCookieName = "HangFireCookie";
    private const int CookieExpirationMinutes = 60;

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        string? accessToken;
        var setCookie = false;

        if (httpContext.Request.Query.ContainsKey("access_token"))
        {
            accessToken = httpContext.Request.Query["access_token"].FirstOrDefault();
            setCookie = true;
        }
        else
        {
            accessToken = httpContext.Request.Cookies[HangFireCookieName];
        }

        if (string.IsNullOrEmpty(accessToken)) return false;

        var hand = new JwtSecurityTokenHandler();
        var claims = hand.ValidateToken(accessToken, tokenValidationParameters, out _);
        if (!string.IsNullOrEmpty(role) && !claims.IsInRole(role)) return false;

        if (setCookie)
            httpContext.Response.Cookies.Append(HangFireCookieName,
                accessToken,
                new CookieOptions
                {
                    Expires = DateTime.Now.AddMinutes(CookieExpirationMinutes)
                });
        return true;
    }
}