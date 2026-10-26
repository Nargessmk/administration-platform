using System.ComponentModel.DataAnnotations.Schema;

namespace Cloudland.APM.Models;

[Table("tblUserConstantPermission")]
public class UserConstantPermission : BaseModel<Guid>
{
    public Guid? UserId { get; set; }
    public Guid? RoleId { get; set; }
    public Guid? GroupId { get; set; }

    public bool HasPermission { get; set; }

    public Guid? ConstantId { get; set; }
    public Constant? Constant { get; set; }
}