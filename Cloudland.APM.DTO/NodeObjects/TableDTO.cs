namespace Cloudland.APM.DTO.NodeObjects;

public class TableDTO : ValidatableNodeObjectDTO<Guid>
{
    public required string FullName { get; set; }
    public required string DisplayName { get; set; }
    public required string Comment { get; set; } //پیغام نمایشی
    public string? TABLESCHEMA { get; set; } //dbo
    //public long CoreObjectID { get; set; } //???
    public int ShowRecordCountDefault { get; set; }
    public string? AnalystDescription { get; set; } //تحلیل جدول

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نام کامل جدول",
            Name = nameof(FullName),
            InputType = "text",
            IsRequired = true,
            Icon = "table",
            Placeholder = "نام کامل جدول را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Display Settings
        new()
        {
            Title = "نام نمایشی",
            Name = nameof(DisplayName),
            InputType = "text",
            IsRequired = false,
            Icon = "display-name",
            Placeholder = "نام نمایشی جدول را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "پیغام نمایشی",
            Name = nameof(Comment),
            InputType = "text",
            IsRequired = false,
            Icon = "comment",
            Placeholder = "پیغام نمایشی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Database Configuration
        new()
        {
            Title = "نام کاربر امنیتی جدول",
            Name = nameof(TABLESCHEMA),
            InputType = "text",
            IsRequired = false,
            Icon = "schema",
            Placeholder = "نام schema را وارد کنید (مثلاً dbo)",
            IsReadonly = false,
            Disabled = false
        },

        // Table Behavior
        new()
        {
            Title = "تعداد نمایش رکورد پیش‌فرض",
            Name = nameof(ShowRecordCountDefault),
            InputType = "number",
            IsRequired = false,
            Icon = "record-count",
            Placeholder = "تعداد رکوردهای نمایش داده شده را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Developer Notes
        new()
        {
            Title = "توضیحات تحلیلگر",
            Name = nameof(AnalystDescription),
            InputType = "textarea",
            IsRequired = false,
            Icon = "analysis",
            Placeholder = "توضیحات تحلیل و برنامه‌نویسی را وارد کنید",
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
        //     Placeholder = "",
        //     IsReadonly = true,
        //     Disabled = true
        // }
    ];
}