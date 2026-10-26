namespace Cloudland.SSSB.Models.View;

public class FoodView
{
    public Guid Id { get; set; }
    public Guid? FoodCategoryId { get; set; }
    public string? FoodCategoryTitle { get; set; }
    public Guid MealId { get; set; }
    public string? MealTitle { get; set; }
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

    // From Constant table
    public Guid? ParentId { get; set; }
}
