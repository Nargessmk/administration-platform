using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class RoomDTO : BaseDTO<Guid>
{
    public Guid? LocationId { get; set; }
    public string? Number { get; set; }
    public Guid? RoomStatusId { get; set; }
    public int? EmptyBedCount { get; set; }
    public Guid? RoomTypeId { get; set; }
}