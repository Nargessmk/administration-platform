namespace Cloudland.SSSB.DTO.ViewDTO;

public class ReservationRequestBedViewDTO
{
    public Guid Id { get; set; }
    public Guid? ReservationRequestHeaderId { get; set; }
    public Guid? BedId { get; set; }
    public Guid? PersonnelId { get; set; }
    public string? ReservationDate { get; set; }
    public bool CancellationStatus { get; set; }
    public Guid? AdmissionStatus { get; set; } //TableLookup
    public string? AdmissionStatusTitle { get; set; }
    public bool? RoomDeliveryStatus { get; set; }
    public string? DeliveryDate { get; set; }
    public string? DeliveryTime { get; set; }
    public string? DocumentReceipt { get; set; }

    // Bed fields
    public Guid? BedStatusId { get; set; }
    public string? BedStatusTitle { get; set; }
    public string? BedNumber { get; set; }
    
    //Personnel Fields
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? NationalCode { get; set; }

    
    //Location fields
    public Guid? LocationId { get; set; }
    public string? LocationTitle { get; set; }
    
    
    public string CreateAt { get; set; } = string.Empty;
    public string LastModifiedAt { get; set; } = string.Empty;
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



