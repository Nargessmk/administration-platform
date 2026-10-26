namespace Cloudland.APM.Models.Auth;

public class ResetPasswordRequest
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? newPassword { get; set; }
}