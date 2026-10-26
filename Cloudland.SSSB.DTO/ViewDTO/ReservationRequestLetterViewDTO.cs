namespace Cloudland.SSSB.DTO.ViewDTO;

public class ReservationRequestLetterViewDTO
{
    public Guid Id { get; set; }
    public Guid? ReservationRequestHeaderId { get; set; }
    public string? LetterDate { get; set; }
    public string LetterNumber { get; set; } = string.Empty;
    public string LetterSubject { get; set; } = string.Empty;
    public string? LetterFileUrl { get; set; }

    public string? FromDate { get; set; }
    public string? ToDate { get; set; }

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


