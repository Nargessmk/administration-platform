using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class ReservationRequestBed : BaseModel<Guid>
{
	[Required]
	[Description("شناسه درخواست رزرو غذا")]
	public Guid? ReservationRequestHeaderId { get; set; }
	[ForeignKey(nameof(ReservationRequestHeaderId))]
	public ReservationRequestHeader? ReservationRequestHeader { get; set; }

	[Description("شناسه تخت")]
	public Guid? BedId { get; set; }
	
	[Description("شناسه پرسنل")]
	public Guid? PersonnelId { get; set; }

	[Required]
	[Description("تاریخ رزرو")]
	public DateTime? ReservationDate { get; set; }
	
	[Required]
	[Description("وضعیت پذیرش")]
	public Guid? AdmissionStatus { get; set; } //TableLookup

	[Required]
	[Description("وضعیت تحویل اتاق")]
	public bool? RoomDeliveryStatus { get; set; }
	
	[Description("تاریخ تحویل اتاق")]
	public DateTime? DeliveryDate { get; set; }
	
	[MaxLength(100)]
	[Description("ساعت تحویل اتاق")]
	public string? DeliveryTime { get; set; }

	[Required]
	[Description("وضعیت لغو")]
	public bool CancellationStatus { get; set; }
	
	[MaxLength(400)]
	[Description("مدرک دریافت شده")]
	public string? DocumentReceipt { get; set; }
}
