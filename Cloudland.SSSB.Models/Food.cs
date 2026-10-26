using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class Food : BaseModel<Guid>
{
    [ForeignKey(nameof(Id))]
    public Constant? Constant { get; set; }
    
    [Required]
    [Description("دسته بندی غذا")]
    public Guid? FoodCategoryId { get; set; } //Table Lookup

    [Required]
    [Description("شناسه وعده مرتبط")]
    public Guid MealId { get; set; }
    
    public virtual ICollection<ContractFood>? ContractFoods { get; set; }
}