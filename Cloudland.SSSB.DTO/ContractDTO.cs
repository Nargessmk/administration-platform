using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class ContractDTO : BaseDTO<Guid>
{
    public Guid ContractorId { get; set; }
    public Guid? ParentId { get; set; }
    public string? StartDate { get; set; } 
    public string? EndDate { get; set; } 
    public string? ContractNumber { get; set; } 
    public bool IsActive { get; set; } 
    public int Quantity { get; set; } 
    public decimal Price { get; set; } 
    public decimal AdjustmentPercentage { get; set; } 
    public Guid? ContractTypeId { get; set; } 
}