using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class ShiftWorkerRequestDTO : BaseDTO<Guid>
{
    public Guid ReservationRequestHeaderId { get; set; }
    public string? ShiftChangeDay { get; set; } = string.Empty;
    public bool IsHoliday { get; set; }
    public bool IsThursday { get; set; }
    public Guid MealId { get; set; }
    public int FoodCount { get; set; }
}