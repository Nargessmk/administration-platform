namespace Cloudland.APM.DTO;

public abstract class BaseDTO<T>
{
    public T? Id { get; set; }
    public string CreateAt { get; set; } = string.Empty;
    public string LastModifiedAt { get; set; } = string.Empty;
    public Guid UserCreated { get; set; }
    public Guid LastModifiedUser { get; set; }


    public string? CustomerId { get; set; }


    public string? title { get; set; }
    public string? desc { get; set; }

    public bool AllowDelete { get; set; }
    public bool AllowUpdate { get; set; }
    public bool Disabled { get; set; }
    public bool hidden { get; set; }
}