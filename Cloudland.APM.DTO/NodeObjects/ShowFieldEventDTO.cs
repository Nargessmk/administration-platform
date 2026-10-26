namespace Cloudland.APM.DTO.NodeObjects;

public class ShowFieldEventDTO : ValidatableNodeObjectDTO<Guid>
{
    public required string FieldValue { get; set; }
    public long SelectedConstantID { get; set; }
    public bool ShowObject { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "مقدار فیلد",
            Name = nameof(FieldValue),
            InputType = "text",
            IsRequired = false,
            Icon = "field-value",
            Placeholder = "مقدار فیلد را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Object Selection
        new()
        {
            Title = "انتخابی",
            Name = nameof(SelectedConstantID),
            InputType = "selectbox",
            IsRequired = false,
            Icon = "object-select",
            Placeholder = "آیتم را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Visibility Control
        new()
        {
            Title = "وضعیت نمایش",
            Name = nameof(ShowObject),
            InputType = "switch",
            IsRequired = false,
            Icon = "visibility",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        }
    ];
}