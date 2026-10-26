using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class VehicleDriverDTO : BaseDTO<Guid>
{
    public Guid? VehicleId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? PersonnelId { get; set; }
}
