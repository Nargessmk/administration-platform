using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class Personnel : BaseModel<Guid>
{
    [ForeignKey(nameof(Id))]
    public Constant? Constant { get; set; }
    
    [Description("شناسه کاربری فیوژن")]
    public Guid? FusionUserId { get; set; } 
    
    [Required] 
    [MaxLength(100)] 
    [Description("نام")]
    public string? FirstName { get; set; }
    
    [Required] 
    [MaxLength(100)]
    [Description("نام خانوادگی")]
    public string? LastName { get; set; }

    [Required]
    [MaxLength(10)] 
    [Description("کد ملی")]
    public string? NationalCode { get; set; }

    [Required]
    [MaxLength(50)]
    [Description("کد پرسنلی")]
    public string? PersonnelCode { get; set; }

    [Description("شناسه نسبت خانوادگی")]
    public Guid? FamilyRelationTypeId { get; set; } //table lookup

    [Description("تاریخ تولد")]
    public DateTime? DateOfBirth { get; set; }
    
    [MaxLength(20)] 
    [Description("شماره تماس")]
    public string? PhoneNumber { get; set; }
    
    [MaxLength(20)] 
    [Description("موبایل ۱")]
    public string? Mobile1 { get; set; }
    
    [MaxLength(20)]
    [Description("موبایل ۲")]
    public string? Mobile2 { get; set; }
        
    [Required] 
    [Description("وضعیت نوبتکار")]
    public bool HasShift { get; set; }
    
    [Required] 
    [Description("وضعیت روزکار")]
    public bool IsDailyWorker { get; set; } 

    [Required]
    [Description("وضعیت رزرو غذا")]
    public bool HasFoodReservation { get; set; } 
        
    [Required]
    [Description("شناسه نوع قرارداد پرسنل")]
    public Guid? ContractTypeId { get; set; } //table lookup

    [Required]
    [Description("وضعیت فعال")]
    public bool IsActive { get; set; } 
        
    [Description("وضعیت بازنشستگی")]
    public bool IsRetired { get; set; } 

    
    [MaxLength(150)]
    [Description("نام نمایشی")]
    public string? DisplayName { get; set; }

    [MaxLength(500)]
    [Description("آدرس")]
    public string? Address { get; set; }
        
    [Description("شناسه سمت سازمانی")]
    public Guid? OrganizationalPositionId { get; set; }
        
    [Required]
    [Description("شناسه ساختار سازمانی")]
    public Guid? OrganizationalChartId { get; set; }

    [Description("امضا")]
    [MaxLength(4000)]
    public string? Signature { get; set; } 

    [Description("کیف پول")]
    public decimal WalletBalance { get; set; }

    [Required]
    [Description("وضعیت مدیر سیستم")]
    public bool IsSupervisor { get; set; }
    
    public virtual ICollection<VehicleDriver>? VehicleDrivers { get; set; }
    public virtual ICollection<ReservationRequestPerson>? ReservationRequestPersons { get; set; }
}