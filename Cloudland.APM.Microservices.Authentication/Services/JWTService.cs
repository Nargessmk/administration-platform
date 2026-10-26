using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using io.fusionauth.domain;
using Cloudland.APM.Models.Auth;
using Cloudland.APM.Models.JWT;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cloudland.APM.Microservices.Authentication.Services;

public class JWTService(IOptions<JwtSettings> jwtSettings, IConfiguration config) : IJwtService
{
    private readonly IConfiguration _config = config;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public string GenerateSecurityToken(LoginModel user, User? fullUserinfo)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Name, fullUserinfo?.username ?? string.Empty),
            new(ClaimTypes.Name, user.userName),
            new(ClaimTypes.Sid, fullUserinfo?.id.ToString()!),
            new(ClaimTypes.Email, fullUserinfo?.email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, fullUserinfo?.fullName ?? string.Empty),
            new(ClaimTypes.MobilePhone, fullUserinfo?.mobilePhone ?? string.Empty),
            new("firstName", fullUserinfo?.firstName ?? string.Empty),
            new("lastName", fullUserinfo?.lastName ?? string.Empty),
            new("lastLoginTime", fullUserinfo?.lastLoginInstant.ToString() ?? string.Empty),
            new("fullName", fullUserinfo?.fullName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, fullUserinfo?.id.ToString() ?? string.Empty)
        };
        claims.AddRange(from userRole in user.roles ?? [] select new Claim(ClaimTypes.Role, userRole));
        var token = new JwtSecurityToken(
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            claims,
            expires: DateTime.Now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}