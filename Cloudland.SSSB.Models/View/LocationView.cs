namespace Cloudland.SSSB.Models.View;

public class LocationView
{
    public Guid Id { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public int? RoomCount { get; set; }
    public bool IsActive { get; set; }
    public Guid? LocationTypeId { get; set; }
    public string? LocationTypeTitle { get; set; }
    public bool IsReservationActive { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
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

    public Guid? AdminId { get; set; }
}
