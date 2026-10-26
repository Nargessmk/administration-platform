using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class OrganizationalPosition : BaseModel<Guid>
{
    [ForeignKey(nameof(Id))]
    public Constant? Constant { get; set; }
    
    [MaxLength(20)]
    [Description("کد سمت سازمانی")]
    public string? PositionCode { get; set; }
}