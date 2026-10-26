using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class Location : BaseModel<Guid>
{
    [ForeignKey(nameof(Id))]
    public Constant? Constant { get; set; }
    
    [MaxLength(500)]
    [Description("آدرس")]
    public string? Address { get; set; }

    [MaxLength(20)]
    [Description("شماره تماس")]
    public string? PhoneNumber { get; set; }

    [Required]
    [Description("تعداد اتاق")]
    public int? RoomCount { get; set; }

    [Required]
    [Description("وضعیت فعال")]
    public bool IsActive { get; set; }
    
    [Required]
    [Description("شناسه نوع مکان")]
    public Guid? LocationTypeId { get; set; } //Table Lookup

    [Required]
    [Description("وضعیت فعال بودن رزرو")]
    public bool IsReservationActive { get; set; }

    [Description("شناسه ادمین رستوران (لینک شده به مدیریت کاربران)")]
    public Guid? AdminId { get; set; }
    
    public virtual ICollection<Room>? Rooms { get; set; }
    public virtual ICollection<ContractLocation>? ContractLocations { get; set; }
}