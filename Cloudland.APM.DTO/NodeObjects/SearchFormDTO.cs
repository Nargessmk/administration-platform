namespace Cloudland.APM.DTO.NodeObjects;

public class SearchFormDTO : ValidatableNodeObjectDTO<Guid>
{
    public long RelatedTable { get; set; }
    public required string FullName { get; set; }
    public string? Query { get; set; }
    public string? GroupByQuery { get; set; }
    public string? ConditionQuery { get; set; }
    public string? CommonConditionQuery { get; set; }
    //public long CoreObjectID { get; set; }
    public string? SelectedColumns { get; set; }
    public string? Icon { get; set; }
    public bool ShowIcon { get; set; }
    public bool ShowText { get; set; }
    public bool ClearGridAfterSearch { get; set; }
    public string? SearchConditionQuery { get; set; }
    public string? SearchAlarmQuery { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نام فرم جستجو",
            Name = nameof(FullName),
            InputType = "text",
            IsRequired = true,
            Icon = "search-form",
            Placeholder = "نام فرم جستجو را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Table Configuration
        new()
        {
            Title = "جدول مرتبط",
            Name = nameof(RelatedTable),
            InputType = "selectbox",
            IsRequired = true,
            Icon = "table-link",
            Placeholder = "جدول مرتبط را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Appearance Settings
        new()
        {
            Title = "آیکن",
            Name = nameof(Icon),
            InputType = "text",
            IsRequired = true,
            Icon = "image",
            Placeholder = "نام آیکن را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش آیکن",
            Name = nameof(ShowIcon),
            InputType = "switch",
            IsRequired = true,
            Icon = "eye",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش نام",
            Name = nameof(ShowText),
            InputType = "switch",
            IsRequired = true,
            Icon = "text",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },

        // Grid Behavior
        new()
        {
            Title = "خالی کردن جدول بعد از جستجو",
            Name = nameof(ClearGridAfterSearch),
            InputType = "switch",
            IsRequired = true,
            Icon = "grid-clear",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },

        // Query Configuration
        new()
        {
            Title = "کوئری اصلی",
            Name = nameof(Query),
            InputType = "textarea",
            IsRequired = false,
            Icon = "query",
            Placeholder = "کوئری SQL را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "شرط",
            Name = nameof(ConditionQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "condition",
            Placeholder = "شرط SQL را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "شرط مشترک",
            Name = nameof(CommonConditionQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "common-condition",
            Placeholder = "شرط مشترک SQL را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کوئری گروهبندی",
            Name = nameof(GroupByQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "group-by",
            Placeholder = "کوئری GROUP BY را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ستون های انتخابی",
            Name = nameof(SelectedColumns),
            InputType = "textarea",
            IsRequired = false,
            Icon = "columns",
            Placeholder = "ستون ها را با کاما جدا کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Search Configuration
        new()
        {
            Title = "شرط جستجو",
            Name = nameof(SearchConditionQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "search-condition",
            Placeholder = "شرط جستجو SQL را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "هشدار جستجو",
            Name = nameof(SearchAlarmQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "alarm",
            Placeholder = "کوئری هشدار جستجو را وارد کنید",
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
        //     Placeholder = "شناسه شیء اصلی را وارد کنید",
        //     IsReadonly = false,
        //     Disabled = false
        // }
    ];
}