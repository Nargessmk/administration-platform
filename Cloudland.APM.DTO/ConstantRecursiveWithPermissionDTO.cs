using System.ComponentModel.DataAnnotations;
using Cloudland.APM.ShareObjects;

namespace Cloudland.APM.DTO;

public class ConstantRecursiveWithPermissionDTO : BaseDTO<Guid>
{
    public Guid? ParentId { get; set; }

    [MaxLength(15)] public string? ValidStartDate { get; set; }

    [MaxLength(15)] public string? ValidEndDate { get; set; }

    public ConstantValueType? ValueType { get; set; }

    [MaxLength(1000)] public string? ConstantValue { get; set; }

    [MaxLength(50)] public string? Icon { get; set; }

    [MaxLength(50)] public string? Color { get; set; }

    [MaxLength(4000)] public string? RootTitle { get; set; }

    public int level { get; set; }

    [MaxLength(4000)] public string? Rank { get; set; }

    // Permission information
    public bool HasPermission { get; set; }
    public Guid? PermissionId { get; set; }

    // Children
    public List<ConstantRecursiveWithPermissionDTO>? Children { get; set; } = new();
}