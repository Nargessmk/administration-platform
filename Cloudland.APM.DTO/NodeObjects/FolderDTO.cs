namespace Cloudland.APM.DTO.NodeObjects;

public class FolderDTO : ValidatableNodeObjectDTO<Guid>
{
    public required string FullName { get; set; }
    //public long CoreObjectID { get; set; }
    public required string Icon { get; set; } //عنوان آیکن
    public required string IconColor { get; set; } // رنگ آیکن
    public bool IsExpand { get; set; } // قابل گسترش
    public int IconSize { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نام پوشه",
            Name = nameof(FullName),
            InputType = "text",
            IsRequired = true,
            Icon = "folder",
            Placeholder = "نام پوشه را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Icon Settings
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
        new()
        {
            Title = "رنگ آیکن",
            Name = nameof(IconColor),
            InputType = "color",
            IsRequired = false,
            Icon = "palette",
            Placeholder = "رنگ آیکن را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "اندازه آیکن",
            Name = nameof(IconSize),
            InputType = "number",
            IsRequired = false,
            Icon = "ruler",
            Placeholder = "اندازه آیکن را وارد کنید (پیشفرض: 12)",
            IsReadonly = false,
            Disabled = false
        },

        // Behavior
        new()
        {
            Title = "قابل گسترش",
            Name = nameof(IsExpand),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "expand",
            Placeholder = "",
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