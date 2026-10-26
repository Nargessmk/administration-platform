namespace Cloudland.SSSB.Models.View;

public class VehicleView
{
    public Guid Id { get; set; }
    public Guid VehicleTypeId { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
    public string VehicleTypeTitle { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public DateTime ValidityDate { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public Guid UserCreated { get; set; }
    public Guid LastModifiedUser { get; set; }
    public Guid CustomerId { get; set; }
    public string? title { get; set; }
    public string? CompanyLicensePlate { get; set; }
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
