namespace Cloudland.APM.Models.Auth;

public class UserRemoveApplicationRequest
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
}