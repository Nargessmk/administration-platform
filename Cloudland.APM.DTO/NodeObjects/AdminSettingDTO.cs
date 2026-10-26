using System.ComponentModel.DataAnnotations;

namespace Cloudland.APM.DTO.NodeObjects;

public class AdminSettingDTO : ValidatableNodeObjectDTO<Guid>
{
    [Required] public bool ShowUserRegistryInLoginForm { get; set; }

    [Required] public bool ShowDataSourceListInSettingForm { get; set; }

    [Required] public bool ShowSpecialPhraseListInSettingForm { get; set; }

    public bool ShowInformationEntryFormListInSettingForm { get; set; }
    public bool ShowProcessListInSettingForm { get; set; }
    public bool ShowReportListInSettingForm { get; set; }
    public bool ShowDashboardListInSettingForm { get; set; }
    public bool ShowPublicFileListInSettingForm { get; set; }
    public bool ShowConnectWebsiteListInSettingForm { get; set; }
    public bool ShowSMSSettingInSettingForm { get; set; }
    public bool ShowEmailSettingInSettingForm { get; set; }
    public bool ShowPaymentSettingInSettingForm { get; set; }
    public bool ShowPublicSettingInSettingForm { get; set; }
    public bool ShowUserCalendar { get; set; }
    public bool ShowInfoInLogin { get; set; }
    public bool ShowAllRights { get; set; }

    public bool ShowEditingRestrictions { get; set; }
    public bool ShowCanUpdateOnlyUserRegistry { get; set; }
    public bool ShowCanUpdateOneDey { get; set; }
    public bool ShowCanUpdateThreeDey { get; set; }
    public bool ShowCanUpdateOneWeek { get; set; }

    public bool PermissionShowImportExportInForm { get; set; }
    public bool ShowImportExportInAllForm { get; set; }
    public string[]? AllowFormShowImportExport { get; set; }

    public bool PermissionShowCommentInForm { get; set; }
    public bool ShowCommentInAllForm { get; set; }
    public string[]? AllowFormShowComment { get; set; }

    public bool PermissionShowEmailInForm { get; set; }
    public bool ShowEmailInAllForm { get; set; }
    public string[]? AllowFormShowEmail { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        // عمومی (General)
        new()
        {
            Title = "نمایش ثبت نام در فرم ورود",
            Name = nameof(ShowUserRegistryInLoginForm),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "general",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش تقویم برنامه ریزی",
            Name = nameof(ShowUserCalendar),
            InputType = "checkbox",
            Icon = "general",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش اطلاعات در فرم ورود",
            Name = nameof(ShowInfoInLogin),
            InputType = "checkbox",
            Icon = "general",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش کلیه حقوق",
            Name = nameof(ShowAllRights),
            InputType = "checkbox",
            Icon = "general",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },

        // تنظیمات سیستم (System Settings)
        new()
        {
            Title = "نمایش پایگاه داده",
            Name = nameof(ShowDataSourceListInSettingForm),
            InputType = "checkbox",
            Icon = "system",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش عبارت ویژه",
            Name = nameof(ShowSpecialPhraseListInSettingForm),
            InputType = "checkbox",
            Icon = "system",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش فرم ساز",
            Name = nameof(ShowInformationEntryFormListInSettingForm),
            InputType = "checkbox",
            Icon = "system",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش فرآیند ساز",
            Name = nameof(ShowProcessListInSettingForm),
            InputType = "checkbox",
            Icon = "system",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش گزارش ساز",
            Name = nameof(ShowReportListInSettingForm),
            InputType = "checkbox",
            Icon = "system",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش داشبورد ساز",
            Name = nameof(ShowDashboardListInSettingForm),
            InputType = "checkbox",
            Icon = "system",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش فایل های عمومی",
            Name = nameof(ShowPublicFileListInSettingForm),
            InputType = "checkbox",
            Icon = "system",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش ارتباط با وب سایت",
            Name = nameof(ShowConnectWebsiteListInSettingForm),
            InputType = "checkbox",
            Icon = "system",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش ارتباط با پیامک",
            Name = nameof(ShowSMSSettingInSettingForm),
            InputType = "checkbox",
            Icon = "system",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش ارتباط با ایمیل",
            Name = nameof(ShowEmailSettingInSettingForm),
            InputType = "checkbox",
            Icon = "system",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش ارتباط با درگاه پرداخت",
            Name = nameof(ShowPaymentSettingInSettingForm),
            InputType = "checkbox",
            Icon = "system",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش تنظیمات عمومی",
            Name = nameof(ShowPublicSettingInSettingForm),
            InputType = "checkbox",
            Icon = "system",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },

        // تنظیمات ویرایش (Editing Settings)
        new()
        {
            Title = "نمایش محدودیت های ویرایش",
            Name = nameof(ShowEditingRestrictions),
            InputType = "checkbox",
            Icon = "edit",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "ویرایش محدود به کاربر درج کننده",
            Name = nameof(ShowCanUpdateOnlyUserRegistry),
            InputType = "checkbox",
            Icon = "edit",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "ویرایش به مدت یک روز",
            Name = nameof(ShowCanUpdateOneDey),
            InputType = "checkbox",
            Icon = "edit",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "ویرایش به مدت سه روز",
            Name = nameof(ShowCanUpdateThreeDey),
            InputType = "checkbox",
            Icon = "edit",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "ویرایش به مدت یک هفته",
            Name = nameof(ShowCanUpdateOneWeek),
            InputType = "checkbox",
            Icon = "edit",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },

        // تنظیمات import/export
        new()
        {
            Title = "دسترسی نمایش import در فرم",
            Name = nameof(PermissionShowImportExportInForm),
            InputType = "checkbox",
            Icon = "import-export",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش import در تمام فرم ها",
            Name = nameof(ShowImportExportInAllForm),
            InputType = "checkbox",
            Icon = "import-export",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش import",
            Name = nameof(AllowFormShowImportExport),
            InputType = "CoreRelatedTableCheckbox",
            IsRequired = false,
            Icon = "import-export",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },

        // تنظیمات یادداشت (Note Settings)
        new()
        {
            Title = "دسترسی نمایش یادداشت در فرم",
            Name = nameof(PermissionShowCommentInForm),
            InputType = "checkbox",
            Icon = "note",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش یادداشت در تمام فرم ها",
            Name = nameof(ShowCommentInAllForm),
            InputType = "checkbox",
            Icon = "note",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش یادداشت",
            Name = nameof(AllowFormShowComment),
            InputType = "CoreRelatedTableCheckbox",
            IsRequired = false,
            Icon = "note",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },

        // تنظیمات ایمیل (Email Settings)
        new()
        {
            Title = "دسترسی نمایش ایمیل در فرم",
            Name = nameof(PermissionShowEmailInForm),
            InputType = "checkbox",
            Icon = "email",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش ایمیل در تمام فرم ها",
            Name = nameof(ShowEmailInAllForm),
            InputType = "checkbox",
            Icon = "email",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false,
            IsRequired = false
        },
        new()
        {
            Title = "نمایش ایمیل",
            Name = nameof(AllowFormShowEmail),
            InputType = "CoreRelatedTableCheckbox",
            IsRequired = false,
            Icon = "email",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        }
    ];
}