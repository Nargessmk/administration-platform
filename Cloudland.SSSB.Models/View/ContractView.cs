namespace Cloudland.SSSB.Models.View;

public class ContractView
{
    public Guid Id { get; set; }
    public Guid ContractorId { get; set; }
    public string? ContractorTitle { get; set; }
    public DateTime? StartDate { get; set; }
    public Guid? ParentId { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ContractNumber { get; set; }
    public bool IsActive { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal AdjustmentPercentage { get; set; }
    public Guid? ContractTypeId { get; set; }
    public string? ContractTypeTitle { get; set; }
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
}



