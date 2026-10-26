namespace Cloudland.APM.DTO.NodeObjects;

public class ApplicationDTO : ValidatableNodeObjectDTO<Guid>
{
    public Guid? ApplicationId;
    public required string Name;
    public required string? Description;
    public bool? Active;

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "شناسه نرم افزار",
            Name = nameof(ApplicationId),
            InputType = "text",
            IsRequired = false,
            Icon = "general",
            Placeholder = "",
            IsReadonly = true,
            Disabled = false
        },
        new()
        {
            Title = "عنوان نرم افزار",
            Name = nameof(Name),
            InputType = "text",
            IsRequired = true,
            Icon = "general",
            Placeholder = "عنوان نرم افزار را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "توضیحات نرم افزار",
            Name = nameof(Description),
            InputType = "text",
            IsRequired = true,
            Icon = "general",
            Placeholder = "توضیحات نرم افزار را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "فعال",
            Name = nameof(Active),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "general",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
    ];
}