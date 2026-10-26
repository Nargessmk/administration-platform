namespace Cloudland.APM.Models.Auth;

public class UserRoles
{
    public Guid UserId { get; set; }
    public Guid ApplicationId { get; set; }
    public List<string>? Roles { get; set; }
}