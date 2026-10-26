namespace Cloudland.APM.DTO.NodeObjects;

public class FieldDTO : ValidatableNodeObjectDTO<Guid>
{
    public required string FieldName { get; set; }
    public required string DisplayName { get; set; }
    public required string FieldNature { get; set; }
    public InputTypes FieldType { get; set; }
    public FieldDisplayType FieldDisplayType { get; set; }
    public bool IsEditAble { get; set; }
    public bool IsDefaultView { get; set; } //نمایش پیشفرض
    public bool IsRequired { get; set; }
    public long RelatedTable { get; set; }
    public string? ViewCommand { get; set; }
    public string? SpecialValue { get; set; }
    public string? DefaultValue { get; set; } //مقدار پیشفرض
    public bool IsDefaultViewRadioBox { get; set; }
    public bool IsLeftWrite { get; set; } //چپ چین
    public bool IsIdentity { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsTableAttachment { get; set; }
    public bool IsWide { get; set; }
    public bool ActiveOnKeyDown { get; set; } //فعال کردن key down
    public bool IsExclusive { get; set; }
    public bool ShowInForm { get; set; } //نمایش در فرم
    public string? Folder { get; set; }
    //public long CoreObjectID { get; set; }
    public int DigitsAfterDecimal { get; set; } // تعداد اعشار
    public long RelatedField { get; set; }
    public long RelatedFieldCommand { get; set; }
    public string? Class { get; set; }
    public bool ClearAfterChange { get; set; } //پاک کردن مقدار بعد از تغییر
    public string? FieldComment { get; set; }
    public float MinValue { get; set; }
    public float MaxValue { get; set; }
    public string? AutoFillQuery { get; set; } //پر کردن خودکار
    public string? TextColor { get; set; }
    public int SearchAutoCompleteCount { get; set; }
    public string? GridTextColor { get; set; }
    public string? RelatedLink { get; set; }
    public string? ColumnWidth { get; set; }
    public string? AnalystDescription { get; set; } //توضیحات برنامه نویس

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نام فیلد",
            Name = nameof(FieldName),
            InputType = "text",
            IsRequired = true,
            Icon = "field",
            Placeholder = "نام فیلد را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نام نمایشی",
            Name = nameof(DisplayName),
            InputType = "text",
            IsRequired = false,
            Icon = "text-field",
            Placeholder = "نام نمایشی فیلد را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
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
            Title = "نوع نمایش فیلد",
            Name = nameof(FieldDisplayType),
            InputType = "selectbox",
            IsRequired = false,
            Icon = "display",
            Placeholder = "نوع نمایش را انتخاب کنید",
            IsReadonly = false,
            Disabled = false,
            Options = Enum.GetValues(typeof(FieldDisplayType))
                .Cast<FieldDisplayType>()
                .Select(v => new DropdownOption 
                { 
                    Label = v.ToString(), 
                    Value = (int)v 
                })
                .ToList()
        },
        new()
        {
            Title = "توضیحات فیلد",
            Name = nameof(FieldComment),
            InputType = "text",
            IsRequired = false,
            Icon = "comment",
            Placeholder = "توضیحات فیلد را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Field Behavior
        new()
        {
            Title = "قابل ویرایش",
            Name = nameof(IsEditAble),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "edit",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "مشاهده پیشفرض",
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
            Title = "ضروری",
            Name = nameof(IsRequired),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "required",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "چپ چین",
            Name = nameof(IsLeftWrite),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "align-left",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "منحصر به فرد",
            Name = nameof(IsExclusive),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "unique",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
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
            Title = "فعال کردن KeyDown",
            Name = nameof(ActiveOnKeyDown),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "keyboard",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "پاک کردن فرم بعد از تغییر",
            Name = nameof(ClearAfterChange),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "clear",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },

        // Field Relationships
        new()
        {
            Title = "جدول مرتبط",
            Name = nameof(RelatedTable),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "table",
            Placeholder = "جدول مرتبط را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "فیلد مرتبط",
            Name = nameof(RelatedField),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "field-link",
            Placeholder = "فیلد مرتبط را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "شرط فیلد مرتبط",
            Name = nameof(RelatedFieldCommand),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "field-condition",
            Placeholder = "شرط فیلد مرتبط را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Values and Formatting
        new()
        {
            Title = "مقدار پیشفرض",
            Name = nameof(DefaultValue),
            InputType = "text",
            IsRequired = false,
            Icon = "default",
            Placeholder = "مقدار پیشفرض را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "مقدار ویژه",
            Name = nameof(SpecialValue),
            InputType = "textarea",
            IsRequired = false,
            Icon = "special",
            Placeholder = "مقدار ویژه را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "تعداد رقم بعد اعشار",
            Name = nameof(DigitsAfterDecimal),
            InputType = "number",
            IsRequired = false,
            Icon = "decimal",
            Placeholder = "تعداد ارقام اعشار را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "بیشترین مقدار",
            Name = nameof(MaxValue),
            InputType = "number",
            IsRequired = false,
            Icon = "max",
            Placeholder = "حداکثر مقدار را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کمترین مقدار",
            Name = nameof(MinValue),
            InputType = "number",
            IsRequired = false,
            Icon = "min",
            Placeholder = "حداقل مقدار را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Display Settings
        new()
        {
            Title = "عرض فیلد",
            Name = nameof(ColumnWidth),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "width",
            Placeholder = "عرض فیلد را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "رنگ متن",
            Name = nameof(TextColor),
            InputType = "color",
            IsRequired = false,
            Icon = "text-color",
            Placeholder = "رنگ متن را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "رنگ متن گرید",
            Name = nameof(GridTextColor),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "grid-color",
            Placeholder = "رنگ متن گرید را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "لینک مرتبط",
            Name = nameof(RelatedLink),
            InputType = "url",
            IsRequired = false,
            Icon = "link",
            Placeholder = "لینک مرتبط را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کلاس",
            Name = nameof(Class),
            InputType = "text",
            IsRequired = false,
            Icon = "css",
            Placeholder = "کلاس CSS را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Advanced Settings
        new()
        {
            Title = "دستور نمایشی",
            Name = nameof(ViewCommand),
            InputType = "textarea",
            IsRequired = false,
            Icon = "code",
            Placeholder = "دستور نمایشی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "دستور پر کردن خودکار",
            Name = nameof(AutoFillQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "auto-fill",
            Placeholder = "دستور پر کردن خودکار را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "تعداد کارکتر جهت جستجو",
            Name = nameof(SearchAutoCompleteCount),
            InputType = "number",
            IsRequired = false,
            Icon = "search",
            Placeholder = "تعداد کاراکتر برای جستجو را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "توضیحات برنامه نویس/تحلیلگر",
            Name = nameof(AnalystDescription),
            InputType = "textarea",
            IsRequired = false,
            Icon = "developer",
            Placeholder = "توضیحات فنی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        }
    ];
}