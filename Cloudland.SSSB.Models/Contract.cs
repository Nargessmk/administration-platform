using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class Contract : BaseModel<Guid>
{
    [Required]
    [Description("شناسه پیمانکار مرتبط ")]
    public Guid ContractorId { get; set; }
    [ForeignKey(nameof(ContractorId))]
    public Contractor? Contractor { get; set; }

    [Required]
    [Description("تاریخ شروع قرارداد")]
    public DateTime? StartDate { get; set; } 

    [Description("شناسه قرارداد والد")]
    public Guid? ParentId { get; set; }
    public Contract? ParentContract { get; set; }
    
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
    public Guid? ContractTypeId { get; set; } //Table Lookup

    public virtual ICollection<ContractFood>? ContractFoods { get; set; }
    public virtual ICollection<ContractLocation>? ContractLocations { get; set; }
}