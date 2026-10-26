using System.ComponentModel.DataAnnotations;
using SoftFluent.ComponentModel.DataAnnotations;

namespace Cloudland.APM.Models;

public abstract class BaseModel<T>
{
    [Key] public required T Id { get; set; }

    [Required(ErrorMessage = "تاریخ ایجاد تعیین نشده است")]
    public DateTime CreateAt { get; set; }

    [Required(ErrorMessage = "تاریخ آخرین تغییر تعیین نشده است")]
    public DateTime LastModifiedAt { get; set; }

    [Required(ErrorMessage = "کاربر ایجاد کننده تعیین نشده است")]
    public Guid UserCreated { get; set; }

    [Required(ErrorMessage = "آخرین کاربر تغییر دهنده مشخص نشده است")]
    public Guid LastModifiedUser { get; set; }

    [Encrypted] [Required] public Guid CustomerId { get; set; }

    [Required(ErrorMessage = "عنوان وارد نشده است")]
    public string? title { get; set; }

    [Required(ErrorMessage = "مجوز حذف تعیین نشده است")]
    public bool AllowDelete { get; set; }

    [Required(ErrorMessage = "محوز ویرایش تعیین نشده است ")]
    public bool AllowUpdate { get; set; }

    [Required(ErrorMessage = "وضعیت فعال/غیرفعال مشخص نشده است ")]
    public bool Disabled { get; set; }

    [Required(ErrorMessage = "وضعیت نمایش/عدم نمایش مشخص نشده است")]
    public bool hidden { get; set; }

    [Timestamp] public byte[]? RowVersion { get; set; }

    [ConcurrencyCheck] public Guid Version { get; set; }

    public string? desc { get; set; }
}