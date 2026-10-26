namespace Cloudland.APM.Models;

public class ConstantRecursiveWithPermission : ConstantRecursive
{
    public Guid? PermissionId { get; set; }
    public UserConstantPermission? Permission { get; set; }
    public List<ConstantRecursiveWithPermission>? Children { get; set; }
}