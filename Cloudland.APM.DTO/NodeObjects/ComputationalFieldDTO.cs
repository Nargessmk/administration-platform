namespace Cloudland.APM.DTO.NodeObjects;

public class ComputationalFieldDTO : ValidatableNodeObjectDTO<Guid>
{
    public string? Query { get; set; }
    public bool ShowInForm { get; set; }
    public bool IsDefaultView { get; set; }
    public bool IsWide { get; set; }
    public InputTypes FieldType { get; set; }
    public int DigitsAfterDecimal { get; set; }
    public float MaxValue { get; set; }
    public float MinValue { get; set; }
    public string? DisplayName { get; set; }
    public string? FieldComment { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "کوئری محاسباتی",
            Name = nameof(Query),
            InputType = "textarea",
            IsRequired = true,
            Icon = "code",
            Placeholder = "کوئری محاسباتی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نام نمایشی",
            Name = nameof(DisplayName),
            InputType = "text",
            IsRequired = true,
            Icon = "text-field",
            Placeholder = "نام نمایشی فیلد را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "توضیحات فیلد",
            Name = nameof(FieldComment),
            InputType = "textarea",
            IsRequired = false,
            Icon = "comment",
            Placeholder = "توضیحات فیلد را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Visibility Options
        new()
        {
            Title = "نمایش در فرم",
            Name = nameof(ShowInForm),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "form",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش پیش فرض",
            Name = nameof(IsDefaultView),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "eye",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش عریض",
            Name = nameof(IsWide),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "width",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },

        // Field Type Configuration
        new()
        {
            Title = "نوع فیلد",
            Name = nameof(FieldType),
            InputType = "selectbox",
            IsRequired = true,
            Icon = "type",
            Placeholder = "نوع فیلد را انتخاب کنید",
            IsReadonly = false,
            Disabled = false,
            Options = Enum.GetValues(typeof(InputTypes))
                .Cast<InputTypes>()
                .Select(v => new DropdownOption 
                { 
                    Label = v.ToString(), 
                    Value = (int)v 
                })
                .ToList()
        },
        new()
        {
            Title = "اعداد بعد از اعشار",
            Name = nameof(DigitsAfterDecimal),
            InputType = "number",
            IsRequired = false,
            Icon = "decimal",
            Placeholder = "تعداد ارقام اعشار را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Value Constraints
        new()
        {
            Title = "حداقل مقدار",
            Name = nameof(MinValue),
            InputType = "number",
            IsRequired = false,
            Icon = "min",
            Placeholder = "حداقل مقدار مجاز را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "حداکثر مقدار",
            Name = nameof(MaxValue),
            InputType = "number",
            IsRequired = false,
            Icon = "max",
            Placeholder = "حداکثر مقدار مجاز را وارد کنید",
            IsReadonly = false,
            Disabled = false
        }
    ];
}