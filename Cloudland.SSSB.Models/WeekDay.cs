using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class WeekDay : BaseModel<Guid>
{
    [ForeignKey(nameof(Id))]
    public Constant? Constant { get; set; }

    [Required]
    [Description("ساعت شروع")]
    [MaxLength(50)]
    public string? StartTime { get; set; }

    [Required]
    [Description("ساعت پایان")]
    [MaxLength(50)]
    public string? EndTime { get; set; }
}


