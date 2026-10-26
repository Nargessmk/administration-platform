namespace Cloudland.SSSB.DTO.ViewDTO;

public class ContractFoodViewDTO
{
    public Guid Id { get; set; }
    public Guid? ContractId { get; set; }
    public string? ContractNumber { get; set; }
    public Guid? FoodId { get; set; }
    public string? FoodTitle { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal? FreePrice { get; set; }
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



