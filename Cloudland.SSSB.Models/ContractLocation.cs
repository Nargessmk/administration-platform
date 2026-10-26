using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class ContractLocation : BaseModel<Guid>
{
    [Description("شناسه مکان مرتبط")]
    public Guid? LocationId { get; set; }
    [ForeignKey(nameof(LocationId))]
    public Location? Location { get; set; }
    
    [Description("شناسه قرارداد مرتبط")]
    public Guid? ContractId { get; set; }
    [ForeignKey(nameof(ContractId))]
    public Contract? Contract { get; set; }

    public virtual ICollection<ContractFoodLocation>? ContractFoodLocations { get; set; }
}