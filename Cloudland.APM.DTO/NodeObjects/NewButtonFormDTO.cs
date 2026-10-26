namespace Cloudland.APM.DTO.NodeObjects;

public class NewButtonFormDTO : ValidatableNodeObjectDTO<Guid>
{
    public bool UseUrl { get; set; }
    public string? Url { get; set; }
    public long RelatedInformationForm { get; set; }
    //public long CoreObjectID { get; set; }
    public string? FullName { get; set; }
    public string? Icon { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نام دکمه",
            Name = nameof(FullName),
            InputType = "text",
            IsRequired = true,
            Icon = "button",
            Placeholder = "نام دکمه را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "آیکن",
            Name = nameof(Icon),
            InputType = "icon-picker",
            IsRequired = false,
            Icon = "image",
            Placeholder = "آیکن را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Action Configuration
        new()
        {
            Title = "استفاده از لینک",
            Name = nameof(UseUrl),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "link",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "فرم مرتبط",
            Name = nameof(RelatedInformationForm),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "form",
            Placeholder = "فرم مرتبط را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "آدرس لینک",
            Name = nameof(Url),
            InputType = "url",
            IsRequired = false,
            Icon = "link",
            Placeholder = "آدرس URL را وارد کنید",
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
        //     Icon = "object",
        //     Placeholder = "شناسه شیء اصلی را وارد کنید",
        //     IsReadonly = false,
        //     Disabled = false
        // }
    ];
}