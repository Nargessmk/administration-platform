using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class ReservationFoodCalendarDTO : BaseDTO<Guid>
{
    public Guid? ReservationRequestHeaderId { get; set; }
    public Guid? PersonnelId { get; set; }
    public Guid? ContractFoodCalendarId { get; set; }
    public string? ReservationDate { get; set; }
    public int Quantity { get; set; }
    public bool CancelationStatus { get; set; }
}


