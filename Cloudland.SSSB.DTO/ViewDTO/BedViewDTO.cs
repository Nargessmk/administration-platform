namespace Cloudland.SSSB.DTO.ViewDTO;

public class BedViewDTO
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public string? LocationTitle { get; set; }
    public Guid? BedStatusId { get; set; }
    public string? BedStatusTitle { get; set; }
    public string? BedNumber { get; set; }
    public int? Capacity { get; set; }
    public Guid? ReservationId { get; set; }
    public Guid? BedReservationStatusId { get; set; }
    public string? BedReservationStatusTitle { get; set; }
    public string CreateAt { get; set; } = string.Empty;
    public string LastModifiedAt { get; set; } = string.Empty;
    public Guid UserCreated { get; set; }
    public Guid LastModifiedUser { get; set; }
    public Guid CustomerId { get; set; }
    public string? title { get; set; }
    public bool AllowDelete { get; set; }
    public bool AllowUpdate { get; set; }
    public bool Disabled { get; set; }
    public bool hidden { get; set; }
    public byte[]? RowVersion { get; set; }
    public Guid Version { get; set; }
    public string? desc { get; set; }

    // From Constant table
    public Guid? ParentId { get; set; }
}
