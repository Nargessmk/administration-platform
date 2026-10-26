namespace Cloudland.APM.DTO.NodeObjects;

public class ReportDTO : ValidatableNodeObjectDTO<Guid>
{
    public required string FullName { get; set; }
    public string? Icon { get; set; }
    //public long CoreObjectID { get; set; }
    public long DataSourceID { get; set; }
    public bool ShowInMainMenu { get; set; }
    public bool ShowParameterInLoad { get; set; }
    public bool SelectedRow { get; set; }
    public string? QueryAfterPrint { get; set; }
    public string? QueryBeforeRun { get; set; }
    public string? PrinterName { get; set; }
    public int PrintCopy { get; set; }
    public string? QueryPrintCopy { get; set; }
    public bool UseDefaultIconColor { get; set; }
    public string? IconColor { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نام گزارش",
            Name = nameof(FullName),
            InputType = "text",
            IsRequired = true,
            Icon = "report",
            Placeholder = "نام گزارش را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Appearance Settings
        new()
        {
            Title = "آیکن گزارش",
            Name = nameof(Icon),
            InputType = "text",
            IsRequired = false,
            Icon = "image",
            Placeholder = "نام آیکن را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "استفاده از رنگ پیشفرض",
            Name = nameof(UseDefaultIconColor),
            InputType = "switch",
            IsRequired = false,
            Icon = "color",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "رنگ آیکن",
            Name = nameof(IconColor),
            InputType = "text",
            IsRequired = false,
            Icon = "palette",
            Placeholder = "کد رنگ (مثلاً #FFFFFF)",
            IsReadonly = false,
            Disabled = false
        },

        // Data Source Configuration
        new()
        {
            Title = "منبع داده",
            Name = nameof(DataSourceID),
            InputType = "selectbox",
            IsRequired = false,
            Icon = "database",
            Placeholder = "منبع داده را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Display Settings
        new()
        {
            Title = "نمایش در منوی اصلی",
            Name = nameof(ShowInMainMenu),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "menu",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش پارامترها هنگام لود",
            Name = nameof(ShowParameterInLoad),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "parameters",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "انتخاب ردیف",
            Name = nameof(SelectedRow),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "row-select",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },

        // Printing Configuration
        new()
        {
            Title = "نام پرینتر",
            Name = nameof(PrinterName),
            InputType = "text",
            IsRequired = false,
            Icon = "printer",
            Placeholder = "نام پرینتر را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "تعداد کپی",
            Name = nameof(PrintCopy),
            InputType = "number",
            IsRequired = false,
            Icon = "copy",
            Placeholder = "تعداد را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Query Settings
        new()
        {
            Title = "کوئری تعداد کپی",
            Name = nameof(QueryPrintCopy),
            InputType = "text",
            IsRequired = false,
            Icon = "query",
            Placeholder = "کوئری را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "دستور بعد از چاپ",
            Name = nameof(QueryAfterPrint),
            InputType = "text",
            IsRequired = false,
            Icon = "query-after",
            Placeholder = "دستور SQL را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "دستور قبل از اجرا",
            Name = nameof(QueryBeforeRun),
            InputType = "text",
            IsRequired = false,
            Icon = "query-before",
            Placeholder = "دستور SQL را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // System Reference
        // new()
        // {
        //     Title = "شناسه شیء اصلی",
        //     Name = nameof(CoreObjectID),
        //     InputType = "number",
        //     IsRequired = true,
        //     Icon = "object-id",
        //     Placeholder = "شناسه را وارد کنید",
        //     IsReadonly = false,
        //     Disabled = false
        // }
    ];
}