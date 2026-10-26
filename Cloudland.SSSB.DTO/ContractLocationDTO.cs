using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class ContractLocationDTO : BaseDTO<Guid>
{
    public Guid? LocationId { get; set; }
    public Guid? ContractId { get; set; }
}