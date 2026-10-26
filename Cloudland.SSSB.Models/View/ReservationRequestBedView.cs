namespace Cloudland.SSSB.Models.View;

public class ReservationRequestBedView
{
    public Guid Id { get; set; }
    public Guid? ReservationRequestHeaderId { get; set; }
    public Guid? BedId { get; set; }
    public Guid? PersonnelId { get; set; }
    public DateTime? ReservationDate { get; set; }
    public bool CancellationStatus { get; set; }
    public Guid? AdmissionStatus { get; set; } //TableLookup
    public string? AdmissionStatusTitle { get; set; }
    public bool? RoomDeliveryStatus { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string? DeliveryTime { get; set; }
    public string? DocumentReceipt { get; set; }
    
    //Personnel Fields
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? NationalCode { get; set; }

    // Bed fields
    public Guid? BedStatusId { get; set; }
    public string? BedStatusTitle { get; set; } //TableLookup
    public string? BedNumber { get; set; }
    
    //Location fields
    public Guid? LocationId { get; set; }
    public string? LocationTitle { get; set; }

    public DateTime CreateAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public Guid UserCreated { get; set; }
    public Guid LastModifiedUser { get; set; }
    public Guid CustomerId { get; set; }
    public string? title { get; set; }
    public bool AllowDelete { get; set; }
    public bool AllowUpdate { get; set; }
    public bool Disabled { get; set; }
    public bool hidden { get; set; }
    public byte[]? RowVersion { get; set; }
    public Guid Version { get; set; }
    public string? desc { get; set; }
}



