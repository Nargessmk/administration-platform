using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class ContractFoodLocationDTO : BaseDTO<Guid>
{
    public Guid? ContractFoodId { get; set; }
    public Guid? ContractLocationId { get; set; }
}


