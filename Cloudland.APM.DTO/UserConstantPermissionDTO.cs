namespace Cloudland.APM.DTO;

public class UserConstantPermissionDTO
{
    public Guid? UserId { get; set; }
    public Guid? RoleId { get; set; }
    public Guid? GroupId { get; set; }

    public bool HasPermission { get; set; }

    public Guid? ConstantId { get; set; }
}