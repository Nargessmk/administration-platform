using System.ComponentModel.DataAnnotations;
using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class ContractRecursiveDTO : BaseDTO<Guid>
{
    public Guid? ParentId { get; set; }
    
    [MaxLength(15)] 
    public string? StartDate { get; set; }

    [MaxLength(15)] 
    public string? EndDate { get; set; }

    [MaxLength(50)] 
    public string? ContractNumber { get; set; }

    public bool IsActive { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal AdjustmentPercentage { get; set; }

    public Guid? ContractTypeId { get; set; }
    

    [MaxLength(4000)] 
    public string? RootTitle { get; set; }

    public int level { get; set; }

    [MaxLength(4000)] 
    public string? Rank { get; set; }

    [MaxLength(4000)] 
    public string? HierarchyPath { get; set; }

    public Guid ContractorId { get; set; }
}