using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class LocationDTO : BaseDTO<Guid>
{
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public int? RoomCount { get; set; }
    public bool IsActive { get; set; }
    public Guid? LocationTypeId { get; set; }
    public bool IsReservationActive { get; set; }
}