using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class OrganizationalChartDTO : BaseDTO<Guid>
{
    public string? Address { get; set; }
    
    public string? PhoneNumber { get; set; }

    public string? Body { get; set; }

    public Guid? Head { get; set; } 
    
    public Guid? Alternative { get; set; }
}