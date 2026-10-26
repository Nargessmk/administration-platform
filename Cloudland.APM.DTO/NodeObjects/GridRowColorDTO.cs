namespace Cloudland.APM.DTO.NodeObjects;

public class GridRowColorDTO : ValidatableNodeObjectDTO<Guid>
{
    public long RowColorColumnName { get; set; }
    public string? RowColorColumnFullName { get; set; }
    public string? RowColorOperator { get; set; }
    public string? RowColorColumnValue { get; set; }
    public string? RowColorOperator2 { get; set; }
    public string? RowColorColumnValue2 { get; set; }
    public string? RowColorSelectedColor { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نام کامل ستون",
            Name = nameof(RowColorColumnFullName),
            InputType = "text",
            IsRequired = false,
            Icon = "column",
            Placeholder = "نام کامل ستون را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نام ستون",
            Name = nameof(RowColorColumnName),
            InputType = "dropdown",
            IsRequired = true,
            Icon = "field-select",
            Placeholder = "ستون مورد نظر را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Condition 1
        new()
        {
            Title = "نوع عملگر",
            Name = nameof(RowColorOperator),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "operator",
            Placeholder = "عملگر را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "مقدار ستون",
            Name = nameof(RowColorColumnValue),
            InputType = "text",
            IsRequired = false,
            Icon = "value",
            Placeholder = "مقدار مقایسه را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Condition 2
        new()
        {
            Title = "نوع عملگر دوم",
            Name = nameof(RowColorOperator2),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "operator",
            Placeholder = "عملگر دوم را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "مقدار ستون دوم",
            Name = nameof(RowColorColumnValue2),
            InputType = "text",
            IsRequired = false,
            Icon = "value",
            Placeholder = "مقدار مقایسه دوم را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Color Selection
        new()
        {
            Title = "رنگ سطر",
            Name = nameof(RowColorSelectedColor),
            InputType = "dropdown",
            IsRequired = true,
            Icon = "color",
            Placeholder = "رنگ سطر را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        }
    ];
}