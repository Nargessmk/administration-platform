using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class ContractorDTO : BaseDTO<Guid>
{
    public string? FirstName { get; set; }
    
    public string? LastName { get; set; }
    
    public string? Address { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public string? MobileNumber { get; set; }
    
    public string? PostalCode { get; set; }
    
    public string? AccountNumber { get; set; }
    public string? EconomicNumber { get; set; }
    public string? RegisterNumber { get; set; }
    public string? NationalCode { get; set; }
    
    public bool IsActive { get; set; }
}