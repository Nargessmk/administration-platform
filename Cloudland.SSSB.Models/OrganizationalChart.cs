using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class OrganizationalChart : BaseModel<Guid>
{
    
    [MaxLength(512)]
    [Description("آدرس")]
    public string? Address { get; set; }

    [MaxLength(64)]
    [Description("شماره تماس")]
    public string? PhoneNumber { get; set; }

    [MaxLength(4000)]
    [Description("بدنه")]
    public string? Body { get; set; }

    [Description("رئیس")]
    public Guid? Head { get; set; } 
    
    [Description("جایگزین")]
    public Guid? Alternative { get; set; }
    
    [ForeignKey(nameof(Id))]
    public Constant? Constant { get; set; }

    public virtual ICollection<AccountNumber>? AccountNumbers { get; set; }
    public virtual ICollection<VehicleDriver>? VehicleDrivers { get; set; }
}



