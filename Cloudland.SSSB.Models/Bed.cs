using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class Bed : BaseModel<Guid>
{
    [Required]
    [Description("شناسه اتاق مرتبط")]
    public Guid RoomId { get; set; }
    [ForeignKey(nameof(RoomId))]
    public Room? Room { get; set; }
    
    [Required]
    [Description("شناسه وضعیت تخت")]
    public Guid? BedStatusId { get; set; } //Table Lookup

    [Required]
    [MaxLength(50)]
    [Description("شماره تخت")]
    public string? BedNumber { get; set; }

    [Required]
    [Description("ظرفیت تخت")]
    public int? Capacity { get; set; } 
    
    [Description("شناسه رزرو مرتبط")]
    public Guid? ReservationId { get; set; }
    
    [Required]
    [Description("شناسه وضعیت رزرو تخت")]
    public Guid? BedReservationStatusId { get; set; } //Table Lookup
}