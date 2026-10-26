using io.fusionauth.domain;
using Cloudland.APM.Models.Auth;

namespace Cloudland.APM.Microservices.Authentication.Services;

public interface IJwtService
{
    string GenerateSecurityToken(LoginModel user, User? fullUserInfo);
    string GenerateRefreshToken();
}