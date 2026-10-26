namespace Cloudland.APM.DTO.NodeObjects;

public class SearchFieldDTO : ValidatableNodeObjectDTO<Guid>
{
    public long RelatedField { get; set; }
    public string? DefaultOperator { get; set; }
    public string? DefaultValue { get; set; }
    public bool ShowField { get; set; }
    public bool ShowOperator { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "فیلد مرتبط",
            Name = nameof(RelatedField),
            InputType = "selectbox",
            IsRequired = true,
            Icon = "field-link",
            Placeholder = "فیلد مرتبط را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Operator Settings
        new()
        {
            Title = "عملگر پیشفرض",
            Name = nameof(DefaultOperator),
            InputType = "selectbox",
            IsRequired = true,
            Icon = "operator",
            Placeholder = "عملگر را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "مقدار پیشفرض",
            Name = nameof(DefaultValue),
            InputType = "text",
            IsRequired = true,
            Icon = "default-value",
            Placeholder = "مقدار پیشفرض را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Visibility Settings
        new()
        {
            Title = "نمایش فیلد",
            Name = nameof(ShowField),
            InputType = "switch",
            IsRequired = true,
            Icon = "eye",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش عملگر",
            Name = nameof(ShowOperator),
            InputType = "switch",
            IsRequired = true,
            Icon = "operator-visibility",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        }
    ];
}