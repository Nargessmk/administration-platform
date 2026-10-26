using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class VehicleDriver : BaseModel<Guid>
{
    [Required]
    public Guid? VehicleId { get; set; }
    [ForeignKey(nameof(VehicleId))]
    [Description("خودرو مرتبط")]
    public Vehicle? Vehicle{ get; set; }
    
    [Required]
    public Guid? DepartmentId { get; set; }
    [ForeignKey(nameof(DepartmentId))]
    [Description("اداره مرتبط")]
    public OrganizationalChart? OrganizationalChart{ get; set; }
    
    [Required]
    public Guid? PersonnelId { get; set; }
    [ForeignKey(nameof(PersonnelId))]
    [Description("پرسنل مرتبط")]
    public Personnel? Personnel{ get; set; }
}
