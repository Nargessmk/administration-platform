namespace Cloudland.APM.DTO.FusionSpecialObjects;

public class UserRolesAndGroups
{
    public Dictionary<Guid, List<RoleInfo>> ApplicationRoles { get; set; } = new();

    public List<Guid> GroupIds { get; set; }
        = [];
}