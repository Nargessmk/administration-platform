using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class ContractFoodCalendarDTO : BaseDTO<Guid>
{
    public Guid? ContractFoodLocationId { get; set; }
    public Guid MealId { get; set; }
    public string Date { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public int Priority { get; set; }
    public bool IsHoliday { get; set; }
    public Guid? DayTypeId { get; set; }
}


