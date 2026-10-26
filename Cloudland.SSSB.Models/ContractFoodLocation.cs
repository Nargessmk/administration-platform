using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class ContractFoodLocation : BaseModel<Guid>
{
    [Description("شناسه غذای قرارداد مرتبط")]
    public Guid? ContractFoodId { get; set; }
    [ForeignKey(nameof(ContractFoodId))]
    public ContractFood? ContractFood { get; set; }

    [Description("شناسه مکان قرارداد مرتبط")]
    public Guid? ContractLocationId { get; set; }
    [ForeignKey(nameof(ContractLocationId))]
    public ContractLocation? ContractLocation { get; set; }
    
    public virtual ICollection<ContractFoodCalendar>? ContractFoodCalendars { get; set; }
}


