using Cloudland.APM.Models.Auth;
using io.fusionauth.domain;

namespace Cloudland.APM.Models.JWT;

public class TokenRequest
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public LoginModel? LoginInfo { get; set; }
    public User? User { get; set; }
}