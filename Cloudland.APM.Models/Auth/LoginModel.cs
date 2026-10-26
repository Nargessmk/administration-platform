namespace Cloudland.APM.Models.Auth;

public class LoginModel
{
    public string applicationId { get; set; } = string.Empty;
    public string userName { get; set; } = null!;
    public string password { get; set; } = null!;
    public string[]? roles { get; set; } = null!;
}