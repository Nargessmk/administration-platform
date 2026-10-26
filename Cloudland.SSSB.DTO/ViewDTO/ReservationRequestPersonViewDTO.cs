namespace Cloudland.SSSB.DTO.ViewDTO;

public class ReservationRequestPersonViewDTO
{
    public Guid Id { get; set; }
    public Guid? PersonnelId { get; set; }
    public Guid? ReservationRequestHeaderId { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? NationalCode { get; set; }
    public string? PersonnelCode { get; set; }
    public Guid? OrganizationalPositionId { get; set; }
    public string? OrganizationalPositionTitle { get; set; }
    public Guid? OrganizationalChartId { get; set; }
    public string? OrganizationalChartTitle { get; set; }

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
}


