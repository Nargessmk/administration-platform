using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class Contractor : BaseModel<Guid>
{
    [MaxLength(64)]
    [Description("نام")]
    public string? FirstName { get; set; }
    
    [MaxLength(64)]
    [Description("نام خانوادگی")]
    public string? LastName { get; set; }
    
    [MaxLength(64)]
    [Description("کد ملی")]
    public string? NationalCode { get; set; }
    
    [MaxLength(512)]
    [Description("آدرس")]
    public string? Address { get; set; }
    
    [MaxLength(64)]
    [Description("شماره تلفن")]
    public string? PhoneNumber { get; set; }
    
    [MaxLength(64)]
    [Description("شماره موبایل")]
    public string? MobileNumber { get; set; }
    
    [MaxLength(10)]
    [Description("کد پستی")]
    public string? PostalCode { get; set; }
    
    [MaxLength(64)]
    [Description("شماره حساب")]
    public string? AccountNumber { get; set; }
    [MaxLength(64)]
    [Description("شماره اقتصادی")]
    public string? EconomicNumber { get; set; }
    
    [MaxLength(64)]
    [Description("شماره ثبت")]
    public string? RegisterNumber { get; set; }
    
    [Required]
    [Description("وضعیت فعال")]
    public bool IsActive { get; set; }
    
    [ForeignKey(nameof(Id))]
    public Constant? Constant { get; set; }
}



