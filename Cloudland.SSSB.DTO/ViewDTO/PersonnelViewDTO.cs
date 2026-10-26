namespace Cloudland.SSSB.DTO.ViewDTO;

public class PersonnelViewDTO
{
    public Guid Id { get; set; }
    public Guid? FusionUserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? NationalCode { get; set; }
    public string? PersonnelCode { get; set; }
    public Guid? FamilyRelationTypeId { get; set; }
    public string? FamilyRelationTypeTitle { get; set; }
    public string? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Mobile1 { get; set; }
    public string? Mobile2 { get; set; }
    public bool HasShift { get; set; }
    public bool IsDailyWorker { get; set; }
    public bool HasFoodReservation { get; set; }
    public Guid ContractTypeId { get; set; }
    public string? ContractTypeTitle { get; set; }
    public bool IsActive { get; set; }
    public bool IsRetired { get; set; }
    public string? DisplayName { get; set; }
    public string? Address { get; set; }
    public Guid? OrganizationalPositionId { get; set; }
    public string? OrganizationalPositionTitle { get; set; }
    public Guid? OrganizationalChartId { get; set; }
    public string? OrganizationalChartTitle { get; set; }
    public string? Signature { get; set; }
    public decimal WalletBalance { get; set; }
    public bool IsSupervisor { get; set; }
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
}
