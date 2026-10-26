using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class ReservationRequestHeaderDTO : BaseDTO<Guid>
{
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
    public Guid? RequestTypeId { get; set; }
    public Guid? AccountNumberId { get; set; }
    public Guid? FromDepartmentId { get; set; }
    public Guid? ForDepartmentId { get; set; }
    public Guid? RequestingOrganizationalUnitId { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? ShiftChangeDayId { get; set; }
    public Guid? LocationId { get; set; }
}


