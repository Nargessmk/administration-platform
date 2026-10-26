namespace Cloudland.APM.Models.Auth;

public class PasswordRequestRequired
{
    public required string id { get; set; }
    public required string changePasswordId { get; set; }
    public required string currentPassword { get; set; }
    public required string password { get; set; }
}