using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class Vehicle : BaseModel<Guid>
{
    [Required]
    [Description("شناسه نوع خودرو")]
    public Guid VehicleTypeId { get; set; } // Table Lookup
    
    [Required]
    [Description("ظرفیت خودرو")]
    public int Capacity { get; set; }
    
    [Required]
    [Description("وضعیت فعال")]
    public bool IsActive { get; set; }
    
    
    [Required]
    [MaxLength(256)]
    [Description("عنوان نوع خودرو")]
    public string VehicleTypeTitle { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    [Description("شماره پلاک")]
    public string LicensePlate { get; set; } = string.Empty;
    
    [Required]
    [Description("تاریخ اعتبار")]
    public DateTime? ValidityDate { get; set; }
    
    [ForeignKey(nameof(Id))]
    public Constant? Constant { get; set; }
    
    public virtual ICollection<VehicleDriver>? VehicleDrivers { get; set; }
}
