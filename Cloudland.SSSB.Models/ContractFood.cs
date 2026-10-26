using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class ContractFood : BaseModel<Guid>
{
    // Foreign key for Contract
    [Required]
    [Description("شناسه قرارداد مرتبط")]
    public Guid? ContractId { get; set; }
    [ForeignKey(nameof(ContractId))]
    public Contract? Contract { get; set; }

    [Required]
    [Description("شناسه غذای مرتبط")]
    public Guid? FoodId { get; set; }
    [ForeignKey(nameof(FoodId))]
    public Food? Food { get; set; }

    [Required]
    [Description("تعداد ارایه شده غذا")]
    public int Quantity { get; set; } 
    

    [Required]
    [Description("قیمت یک عدد غذا")]
    public decimal Price { get; set; } 

    [Required]
    [Description("قیمت غذای آزاد")]
    public decimal? FreePrice { get; set; } 

    public virtual ICollection<ContractFoodLocation>? ContractFoodLocations { get; set; }
}