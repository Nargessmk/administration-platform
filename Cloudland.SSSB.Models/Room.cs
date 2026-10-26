using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class Room : BaseModel<Guid>
{
    [Required]
    [Description("شناسه مکان مرتبط")]
    public Guid? LocationId { get; set; }
    [ForeignKey(nameof(LocationId))]
    public Location? Location { get; set; }
    
    [Required]
    [MaxLength(50)]
    [Description("شماره اتاق")]
    public string? Number { get; set; } 

    [Required]
    [Description("شناسه وضعیت اتاق")]
    public Guid? RoomStatusId { get; set; } //Table Lookup

    [Required]
    [Description("تعداد تخت خالی")]
    public int? EmptyBedCount { get; set; } 
    
    [Required]
    [Description("شناسه نوع اتاق")]
    public Guid? RoomTypeId { get; set; } //Table Lookup
    
    public virtual ICollection<Bed>? Beds { get; set; }
}