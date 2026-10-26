namespace Cloudland.SSSB.DTO.ViewDTO;

public class WeekDayViewDTO
{
    public Guid Id { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
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
    public Guid? ParentId { get; set; }
}