using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class ReservationRequestPersonDTO : BaseDTO<Guid>
{
    public Guid? PersonnelId { get; set; }
    public Guid? ReservationRequestHeaderId { get; set; }
}


