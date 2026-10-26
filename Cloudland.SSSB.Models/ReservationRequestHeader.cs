using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloudland.APM.Models;

namespace Cloudland.SSSB.Models;

public class ReservationRequestHeader : BaseModel<Guid>
{
    [Required]
    [Description("تاریخ از")]
    public DateTime? FromDate { get; set; }

    [Required]
    [Description("تاریخ تا")]
    public DateTime? ToDate { get; set; }

    [Required]
    [Description("نوع درخواست غذا - جدول ثابت")]
    public Guid? RequestTypeId { get; set; } // Table Lookup

    [Required]
    [Description("شناسه شماره حساب")]
    public Guid? AccountNumberId { get; set; }
    [ForeignKey(nameof(AccountNumberId))]
    public AccountNumber? AccountNumber { get; set; }

    [Required]
    [Description("شناسه اداره مبدا")]
    public Guid? FromDepartmentId { get; set; }
    [ForeignKey(nameof(FromDepartmentId))]
    public OrganizationalChart? FromDepartment { get; set; }

    [Required]
    [Description("شناسه اداره مقصد")]
    public Guid? ForDepartmentId { get; set; }

    [Required]
    [Description("شناسه واحد سازمانی درخواست دهنده")]
    public Guid? RequestingOrganizationalUnitId { get; set; }
    [ForeignKey(nameof(RequestingOrganizationalUnitId))]
    public OrganizationalChart? RequestingOrganizationalUnit { get; set; }

    [Description("شناسه خودرو")]
    public Guid? VehicleId { get; set; }

    [Description("شناسه راننده")]
    public Guid? DriverId { get; set; }

    [Required]
    [Description("روز تغییر شیفت - جدول ثابت")]
    public Guid? ShiftChangeDayId { get; set; } // Table Lookup

    [Required]
    [Description("شناسه مکان")]
    public Guid? LocationId { get; set; }
    [ForeignKey(nameof(LocationId))]
    public Location? Location { get; set; }

    public virtual ICollection<ReservationRequestLetter>? ReservationRequestLetters { get; set; }
    public virtual ICollection<ReservationFoodCalendar>? FoodReservations { get; set; }
    public virtual ICollection<ReservationRequestBed>? ReservationRequestBeds { get; set; }
    public virtual ICollection<ReservationRequestPerson>? ReservationRequestPersons { get; set; }
}


