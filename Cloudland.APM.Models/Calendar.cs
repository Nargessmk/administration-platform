using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.APM.Models;

public class Calendar : BaseModel<Guid>
{
    [ForeignKey(nameof(Id))]
    public Constant? Constant { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Description("مناسبت")]
    [MaxLength(4000)]
    public string? Occasion { get; set; }

    [Description("توضیحات اضافی")]
    [MaxLength(4000)]
    public string? AdditionalDescription { get; set; }

    [Description("وضعیت تعطیلی")]
    public bool IsHoliday { get; set; }

    [Description("وضعیت پنج شنبه")]
    public bool IsThursday { get; set; }
    
    [Description("وضعیت جمعه")]
    public bool IsFriday { get; set; }
}


