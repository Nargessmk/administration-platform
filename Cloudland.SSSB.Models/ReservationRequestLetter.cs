using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class ReservationRequestLetter : BaseModel<Guid>
{
    [Required]
    [Description("شناسه درخواست رزرو غذا")]
    public Guid? ReservationRequestHeaderId { get; set; }
    [ForeignKey(nameof(ReservationRequestHeaderId))]
    public ReservationRequestHeader? ReservationRequestHeader { get; set; }

    [Required]
    [Description("تاریخ نامه")]
    public DateTime? LetterDate { get; set; }

    [Required]
    [MaxLength(128)]
    [Description("شماره نامه")]
    public string LetterNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    [Description("موضوع نامه")]
    public string LetterSubject { get; set; } = string.Empty;

    [MaxLength(1024)]
    [Description("آدرس فایل نامه")]
    public string? LetterFileUrl { get; set; }
}


