using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class AccountNumber : BaseModel<Guid>
{
    [Required]
    [Description("شناسه نمودار سازمانی مرتبط")]
    public Guid OrganizationalChartId { get; set; }
    
    [MaxLength(128)]
    [Description("شماره حساب")]
    public string AccountNumberValue { get; set; } = string.Empty;
    
    [ForeignKey(nameof(OrganizationalChartId))]
    [Description("نمودار سازمانی مرتبط")]
    public OrganizationalChart? OrganizationalChart{ get; set; }
}



