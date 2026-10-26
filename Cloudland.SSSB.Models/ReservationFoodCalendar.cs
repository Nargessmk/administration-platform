using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class ReservationFoodCalendar : BaseModel<Guid>
{
	[Required]
	[Description("شناسه درخواست رزرو غذا")]
	public Guid? ReservationRequestHeaderId { get; set; }
	[ForeignKey(nameof(ReservationRequestHeaderId))]
	public ReservationRequestHeader? ReservationRequestHeader { get; set; }

	[Required]
	[Description("شناسه پرسنل")]
	public Guid? PersonnelId { get; set; }

	[Required]
	[Description("شناسه تقویم غذای قرارداد")]
	public Guid? ContractFoodCalendarId { get; set; }
	[ForeignKey(nameof(ContractFoodCalendarId))]
	public ContractFoodCalendar? ContractFoodCalendar { get; set; }

	[Required]
	[Description("تاریخ رزرو")]
	public DateTime? ReservationDate { get; set; }

	[Required]
	[Description("تعداد")]
	public int Quantity { get; set; }

	[Required]
	[Description("وضعیت لغو")]
	public bool CancelationStatus { get; set; }
}
