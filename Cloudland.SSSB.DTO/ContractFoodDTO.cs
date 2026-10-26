using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class ContractFoodDTO : BaseDTO<Guid>
{
    public Guid? ContractId { get; set; }
    public Guid? FoodId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal? FreePrice { get; set; }
}