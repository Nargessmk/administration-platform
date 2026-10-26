using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class AccountNumberDTO : BaseDTO<Guid>
{
    public Guid OrganizationalChartId { get; set; }
    public string AccountNumberValue { get; set; } = string.Empty;
}



