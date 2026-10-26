using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class ReservationRequestLetterDTO : BaseDTO<Guid>
{
    public Guid? ReservationRequestHeaderId { get; set; }
    public string? LetterDate { get; set; }
    public string LetterNumber { get; set; } = string.Empty;
    public string LetterSubject { get; set; } = string.Empty;
    public string? LetterFileUrl { get; set; }
}


