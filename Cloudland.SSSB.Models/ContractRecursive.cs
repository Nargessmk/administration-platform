using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class ContractRecursive : BaseModel<Guid>
{
    public Guid? ParentId { get; set; }
    
    [Required]
    [Description("شناسه پیمانکار مرتبط")]
    public Guid ContractorId { get; set; }

    [Required]
    [Description("تاریخ شروع قرارداد")]
    public DateTime? StartDate { get; set; }

    [Required]
    [Description("تاریخ پایان قرارداد")]
    public DateTime? EndDate { get; set; }

    [Required]
    [Description("شماره قرارداد")]
    [MaxLength(50)]
    public string? ContractNumber { get; set; }

    [Required]
    [Description("وضعیت فعال بودن قرارداد")]
    public bool IsActive { get; set; }

    [Required]
    [Description("تعداد کل پرس غذا")]
    public int Quantity { get; set; }

    [Required]
    [Description("مبلغ کل قرارداد")]
    public decimal Price { get; set; }

    [Required]
    [Description("درصد تعدیل")]
    public decimal AdjustmentPercentage { get; set; }

    [Required]
    [Description("شناسه نوع قرارداد")]
    public Guid? ContractTypeId { get; set; }

    [MaxLength(500)]
    public string? RootTitle { get; set; }
    
    public int Level { get; set; }
    
    [MaxLength(500)]
    public string? Rank { get; set; }
    
    [MaxLength(500)]
    public string? HierarchyPath { get; set; }
}
