using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class ReservationRequestBedDTO : BaseDTO<Guid>
{
    public Guid? ReservationRequestHeaderId { get; set; }
    public Guid? BedId { get; set; }
    public Guid? PersonnelId { get; set; } 
    public string? ReservationDate { get; set; }
    public bool CancellationStatus { get; set; }
    public Guid? AdmissionStatus { get; set; } //TableLookup
    public bool? RoomDeliveryStatus { get; set; }
    public string? DeliveryDate { get; set; }
    public string? DeliveryTime { get; set; }
    public string? DocumentReceipt { get; set; }
}


