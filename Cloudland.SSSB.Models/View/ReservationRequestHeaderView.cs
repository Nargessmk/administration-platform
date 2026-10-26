namespace Cloudland.SSSB.Models.View;

public class ReservationRequestHeaderView
{
    public Guid Id { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Guid? RequestTypeId { get; set; }
    public string? RequestTypeTitle { get; set; }
    public Guid? AccountNumberId { get; set; }
    public string? AccountNumberTitle { get; set; }
    public string? AccountNumberValue { get; set; }
    public Guid? FromDepartmentId { get; set; }
    public string? FromDepartmentTitle { get; set; }
    public Guid? ForDepartmentId { get; set; }
    public string? ForDepartmentTitle { get; set; }
    public Guid? RequestingOrganizationalUnitId { get; set; }
    public string? RequestingOrganizationalUnitTitle { get; set; }
    public Guid? VehicleId { get; set; }
    public string? VehicleTitle { get; set; }
    public Guid? DriverId { get; set; }
    public string? DriverTitle { get; set; }
    public Guid? ShiftChangeDayId { get; set; }
    public string? ShiftChangeDayTitle { get; set; }
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
	
	// Aggregated fields for list outputs
	public int RegisteredPersonnelCount { get; set; }
	public string? SelectedMeals { get; set; }
	public int ReservedRoomsCount { get; set; }
	public int ReservedAndAdmittedRoomsCount { get; set; }

	// Latest letter fields
	public DateTime? LetterDate { get; set; }
	public string LetterNumber { get; set; } = string.Empty;
	public string LetterSubject { get; set; } = string.Empty;
	public string? LetterFileUrl { get; set; }
}


