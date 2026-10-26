using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class BedDTO : BaseDTO<Guid>
{
    public Guid RoomId { get; set; }
    public Guid? BedStatusId { get; set; }
    public string? BedNumber { get; set; }
    public int? Capacity { get; set; }
    public Guid? ReservationId { get; set; }
    public Guid? BedReservationStatusId { get; set; }
}