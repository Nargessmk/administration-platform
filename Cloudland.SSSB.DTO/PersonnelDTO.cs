using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class PersonnelDTO : BaseDTO<Guid>
{
    public Guid? FusionUserId { get; set; } 
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? NationalCode { get; set; }
    public string? PersonnelCode { get; set; }
    public Guid? FamilyRelationTypeId { get; set; }
    public string? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Mobile1 { get; set; }
    public string? Mobile2 { get; set; }
    public bool HasShift { get; set; }
    public bool IsDailyWorker { get; set; }
    public bool HasFoodReservation { get; set; }
    public Guid? ContractTypeId { get; set; }
    public bool IsActive { get; set; }
    public bool IsRetired { get; set; }
    public string? DisplayName { get; set; }
    public string? Address { get; set; }
    public Guid? OrganizationalPositionId { get; set; }
    public Guid? OrganizationalChartId { get; set; } // Note: Required in model
    public string? Signature { get; set; }
    public decimal WalletBalance { get; set; }
    public bool IsSupervisor { get; set; }
}