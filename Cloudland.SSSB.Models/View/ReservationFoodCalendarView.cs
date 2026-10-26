namespace Cloudland.SSSB.Models.View;

public class ReservationFoodCalendarView
{
    public Guid Id { get; set; }
    public Guid? ReservationRequestHeaderId { get; set; }
    public Guid? PersonnelId { get; set; }
    public Guid? ContractFoodCalendarId { get; set; }
    public DateTime? ReservationDate { get; set; }
    public int Quantity { get; set; }
    public bool CancelationStatus { get; set; }

    // From ContractFoodCalendar
    public Guid? ContractFoodLocationId { get; set; }
    public Guid? FoodId { get; set; }
    public string? FoodTitle { get; set; }
    public Guid? LocationId { get; set; }
    public string? LocationTitle { get; set; }
    public Guid? MealId { get; set; }
    public string? MealTitle { get; set; }
    public DateTime? ContractFoodCalendarDate { get; set; }
    public bool IsDefault { get; set; }
    public int Priority { get; set; }
    public bool IsHoliday { get; set; }
    public Guid? DayTypeId { get; set; }
    public string? DayTypeTitle { get; set; }

    // From Personnel
    public string? PersonnelFirstName { get; set; }
    public string? PersonnelLastName { get; set; }
    public string? PersonnelCode { get; set; }
    public string? PersonnelNationalCode { get; set; }

    // From ReservationRequestHeader
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public DateTime CreateAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public Guid UserCreated { get; set; }
    public Guid LastModifiedUser { get; set; }
    public Guid CustomerId { get; set; }
    public string? title { get; set; }
    public bool AllowDelete { get; set; }
    public bool AllowUpdate { get; set; }
    public bool Disabled { get; set; }
    public bool hidden { get; set; }
    public byte[]? RowVersion { get; set; }
    public Guid Version { get; set; }
    public string? desc { get; set; }
}
