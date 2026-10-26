namespace Cloudland.APM.DTO.NodeObjects;

public class DashboardDTO : ValidatableNodeObjectDTO<Guid>
{
    public required string FullName { get; set; }
    public required string StartDate { get; set; }
    public long InformationEntryForm { get; set; }
    public required string EndDate { get; set; }
    public bool ShowDate { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نام داشبورد",
            Name = nameof(FullName),
            InputType = "text",
            IsRequired = true,
            Icon = "dashboard",
            Placeholder = "نام داشبورد را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        new()
        {
            Title = "فرم ورود اطلاعات",
            Name = nameof(InformationEntryForm),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "form",
            Placeholder = "فرم مرتبط را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Date Settings
        new()
        {
            Title = "تاریخ شروع",
            Name = nameof(StartDate),
            InputType = "date",
            IsRequired = false,
            Icon = "calendar-start",
            Placeholder = "تاریخ شروع را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "تاریخ پایان",
            Name = nameof(EndDate),
            InputType = "date",
            IsRequired = false,
            Icon = "calendar-end",
            Placeholder = "تاریخ پایان را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش تاریخ",
            Name = nameof(ShowDate),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "calendar",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        }
    ];
}