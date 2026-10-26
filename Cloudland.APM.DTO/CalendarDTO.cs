using Cloudland.APM.DTO;

namespace Cloudland.APM.DTO;

public class CalendarDTO : BaseDTO<Guid>
{
    public string Date { get; set; } = string.Empty;
    public string? Occasion { get; set; }
    public string? AdditionalDescription { get; set; }
    public bool IsHoliday { get; set; }
    public bool IsThursday { get; set; }
    public bool IsFriday { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? NodeTypeId { get; set; }
}


