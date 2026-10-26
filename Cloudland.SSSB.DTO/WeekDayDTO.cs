using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class WeekDayDTO : BaseDTO<Guid>
{
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
}


