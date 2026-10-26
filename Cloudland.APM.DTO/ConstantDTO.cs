using System.ComponentModel.DataAnnotations;
using Cloudland.APM.ShareObjects;

namespace Cloudland.APM.DTO;

public class ConstantDTO : BaseDTO<Guid>
{
    public required Guid? ParentId { get; set; }
    public required ConstantDTO? ParentConstant { get; set; }
    public required string? ValidStartDate { get; set; }
    public required string? ValidEndDate { get; set; }
    public ConstantValueType? ValueType { get; set; }
    public required Guid? ConstantType { get; set; }

    [MaxLength(int.MaxValue)] public string? ConstantValue { get; set; }
    public List<ConstantDTO>? Constants { get; set; } = [];

    [MaxLength(100)] public string? Icon { get; set; }
    [MaxLength(100)] public string? Color { get; set; }

    [MaxLength(4000)] public string? JsonData { get; set; }
    
    public bool ChildMustBeShow { get; set; }

    public virtual ICollection<UserConstantPermissionDTO>? UserIndexPermissions { get; set; }
    public Guid? NodeTypeId { get; set; }
    
    public int? Priority { get; set; }
}