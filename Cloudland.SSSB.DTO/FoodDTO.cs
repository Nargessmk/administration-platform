using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class FoodDTO : BaseDTO<Guid>
{
    public Guid? FoodCategoryId { get; set; }
    public Guid MealId { get; set; }
}