using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.ShareObjects;


namespace Cloudland.APM.Models;

[Table("tblConstant")]
public class Constant : BaseModel<Guid>
{
    public Guid? ParentId { get; set; }
    public Constant? ParentConstant { get; set; }
    public DateTime? ValidStartDate { get; set; }
    public DateTime? ValidEndDate { get; set; }
    public ConstantValueType? ValueType { get; set; }
    public Guid? ConstantType { get; set; }

    [MaxLength(int.MaxValue)] public string? ConstantValue { get; set; }
    public List<Constant>? Constants { get; set; } = [];

    [MaxLength(100)] public string? Icon { get; set; }
    [MaxLength(100)] public string? Color { get; set; }

    [MaxLength(4000)] public string? JsonData { get; set; }
    
    public bool ChildMustBeShow { get; set; } = true;

    public virtual ICollection<UserConstantPermission>? UserIndexPermissions { get; set; }
    public Guid? NodeTypeId { get; set; }
    
    public int? Priority { get; set; }
}