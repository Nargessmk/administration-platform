using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class ContractFoodCalendar : BaseModel<Guid>
{
    [Description("شناسه مکان-غذای قرارداد ")]
    public Guid? ContractFoodLocationId { get; set; }

    [ForeignKey(nameof(ContractFoodLocationId))]
    public ContractFoodLocation? ContractFoodLocation { get; set; }

    [Required]
    [Description("شناسه وعده")]
    public Guid MealId { get; set; }
    
    [ForeignKey(nameof(MealId))]
    public Meal? Meal { get; set; }

    [Required]
    [Description("تاریخ")]
    public DateTime Date { get; set; }

    [Required]
    [Description("اولویت")]
    public int Priority { get; set; }

    [Description("تعطیل")]
    public bool IsHoliday { get; set; }

    [Description("پیشفرض")]
    public bool IsDefault { get; set; }

    [Description("نوع روز")]
    public Guid? DayTypeId { get; set; } // Table Lookup
}


