using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class Meal : BaseModel<Guid>
{
    [Required]
    [MaxLength(100)]
    [Description("ساعت شروع")]
    public string? StartTime { get; set; }

    [Required]
    [MaxLength(100)]
    [Description("ساعت پایان")]
    public string? EndTime { get; set; }

    [Required]
    [MaxLength(100)]
    [Description("ساعت پایان رزرو")]
    public string? ReservationEndTime { get; set; }

    [Required]
    [Description("وضعیت فعال")]
    public bool IsActive { get; set; } 
    
    [Description("روز پایان رزرو")]
    public int? ReservationEndDayOffset { get; set; } 

    [MaxLength(100)]
    [Description("ساعت پیشفرض رزرو غذا")]
    public string? DefaultReservationTime { get; set; } 
    
    [Description("روز پیشفرض رزرو غذا")]
    public int? DefaultReservationDayOffset { get; set; } 

    [Description("تاریخ شروع فعال سازی")]
    public DateTime? ActivationStartDate { get; set; }
    
    [Description("تاریخ پایان فعال سازی")]
    public DateTime? ActivationEndDate { get; set; }

    [Description("شناسه نوع وعده غذایی")]
    public Guid? MealCategoryId { get; set; } //Table Lookup

    [Description("وضعیت معوقه")]
    public bool IsPostponed { get; set; } 
    
    [ForeignKey(nameof(Id))]
    public Constant? Constant { get; set; }
    
    public virtual ICollection<ContractFoodCalendar>? ContractFoodCalendars { get; set; }

}