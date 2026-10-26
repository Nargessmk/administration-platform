using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class MealDTO : BaseDTO<Guid>
{
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? ReservationEndTime { get; set; }
    public bool IsActive { get; set; }
    public int? ReservationEndDayOffset { get; set; }
    public string? DefaultReservationTime { get; set; }
    public int? DefaultReservationDayOffset { get; set; }
    public string? ActivationStartDate { get; set; }
    public string? ActivationEndDate { get; set; }
    public Guid? MealCategoryId { get; set; }
    public bool IsPostponed { get; set; }
}