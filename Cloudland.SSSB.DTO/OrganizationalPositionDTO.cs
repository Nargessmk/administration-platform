using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class OrganizationalPositionDTO : BaseDTO<Guid>
{
    public string? PositionCode { get; set; }
}