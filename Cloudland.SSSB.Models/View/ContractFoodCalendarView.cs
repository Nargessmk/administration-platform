namespace Cloudland.SSSB.Models.View;

public class ContractFoodCalendarView
{
    public Guid Id { get; set; }
    public Guid? ContractId { get; set; }
    public Guid FoodId { get; set; }
    public Guid? ContractFoodLocationId { get; set; }
    public string? FoodTitle { get; set; }
    public Guid LocationId { get; set; }
    public string? LocationTitle { get; set; }
    public Guid MealId { get; set; }
    public string? MealTitle { get; set; }
    public DateTime Date { get; set; }
    public bool IsDefault { get; set; }
    public int Priority { get; set; }
    public bool IsHoliday { get; set; }
    public Guid? DayTypeId { get; set; }
    public string? DayTypeTitle { get; set; }
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
