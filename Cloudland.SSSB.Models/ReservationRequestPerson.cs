using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class ReservationRequestPerson : BaseModel<Guid>
{
    [Required]
    [Description("شناسه پرسنل")]
    public Guid? PersonnelId { get; set; }
    [ForeignKey(nameof(PersonnelId))]
    public Personnel? Personnel { get; set; }

    [Required]
    [Description("شناسه درخواست رزرو غذا")]
    public Guid? ReservationRequestHeaderId { get; set; }
    [ForeignKey(nameof(ReservationRequestHeaderId))]
    public ReservationRequestHeader? ReservationRequestHeader { get; set; }
}


