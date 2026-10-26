namespace Cloudland.APM.Models.Auth;

public class PasswordRequest
{
    public required string id { get; set; }
    public required string currentPassword { get; set; }
    public required string password { get; set; }
}