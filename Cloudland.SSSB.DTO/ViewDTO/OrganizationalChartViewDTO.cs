namespace Cloudland.SSSB.DTO.ViewDTO;

public class OrganizationalChartViewDTO
{
    public Guid Id { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Body { get; set; }
    public Guid? Head { get; set; }
    public string? HeadDisplayName { get; set; }
    public Guid? Alternative { get; set; }
    public string? AlternativeDisplayName { get; set; }
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