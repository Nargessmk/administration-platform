namespace Cloudland.SSSB.Models.View;

public class ReservationRequestLetterView
{
    public Guid Id { get; set; }
    public Guid? ReservationRequestHeaderId { get; set; }
    public DateTime? LetterDate { get; set; }
    public string LetterNumber { get; set; } = string.Empty;
    public string LetterSubject { get; set; } = string.Empty;
    public string? LetterFileUrl { get; set; }

    // From ReservationRequestHeader
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

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


