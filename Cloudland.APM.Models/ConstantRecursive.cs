using System.ComponentModel.DataAnnotations;
using Cloudland.APM.ShareObjects;

namespace Cloudland.APM.Models;

public class ConstantRecursive : BaseModel<Guid>
{
    public Guid? ParentId { get; set; }
    public DateTime? ValidStartDate { get; set; }
    public DateTime? ValidEndDate { get; set; }
    public ConstantValueType? ValueType { get; set; }
    
    [MaxLength(500)]
    public string? ConstantValue { get; set; }
    
    [MaxLength(500)]
    public string? Icon { get; set; }
    
    [MaxLength(500)]
    public string? Color { get; set; }
    [MaxLength(500)]
    
    public string? RootTitle { get; set; }
    public int level { get; set; }
    
    [MaxLength(500)]
    public string? Rank { get; set; }
    
    [MaxLength(500)]
    public string? NodeType { get; set; }
    
    public Guid? NodeTypeId { get; set; }
    
    public int? Priority { get; set; }
}