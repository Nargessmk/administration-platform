namespace Cloudland.APM.DTO.NodeObjects;

public class SubDashboardDTO : ValidatableNodeObjectDTO<Guid>
{
    public required string FullName { get; set; }
    //public long CoreObjectID { get; set; }
    public ChartTypes ChartTypes { get; set; }
    public required string Title { get; set; }
    public string? CategoryAxisQuery { get; set; }
    public string? Condition { get; set; }
    public bool IsWide { get; set; }
    public long GroupField { get; set; }
    public long DateField { get; set; }
    public long ParentID { get; set; }
    public int ReloadTime { get; set; }
    public ChartGroupDate ChartGroupDateType { get; set; }
    public ChartCalculationType ChartCalculationType { get; set; }
    public int ColumnSpan { get; set; }
    public int RowSpan { get; set; }
    public long MaxValue { get; set; }
    public long IndicatorValue { get; set; }
    public long MinValue { get; set; }
    public long InformationEntryForm { get; set; }
    public long CalculationField { get; set; }
    public string? Theme { get; set; }
    public string? GroupByQuery { get; set; }
    public string? OrderByQuery { get; set; }
    public string? Icon { get; set; }
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نام زیرداشبورد",
            Name = nameof(FullName),
            InputType = "text",
            IsRequired = true,
            Icon = "dashboard",
            Placeholder = "نام زیرداشبورد را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
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
        // },
        new()
        {
            Title = "عنوان",
            Name = nameof(Title),
            InputType = "text",
            IsRequired = false,
            Icon = "title",
            Placeholder = "عنوان نمودار را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Chart Configuration
        new()
        {
            Title = "نوع نمودار",
            Name = nameof(ChartTypes),
            InputType = "selectbox",
            IsRequired = true,
            Icon = "chart",
            Placeholder = "نوع نمودار را انتخاب کنید",
            IsReadonly = false,
            Disabled = false,
            Options = Enum.GetValues(typeof(ChartTypes))
                .Cast<ChartTypes>()
                .Select(v => new DropdownOption 
                { 
                    Label = v.ToString(), 
                    Value = (int)v 
                })
                .ToList()
        },
        new()
        {
            Title = "نوع محاسبه",
            Name = nameof(ChartCalculationType),
            InputType = "selectbox",
            IsRequired = true,
            Icon = "calculator",
            Placeholder = "نوع محاسبه را انتخاب کنید",
            IsReadonly = false,
            Disabled = false,
            Options = Enum.GetValues(typeof(ChartCalculationType))
                .Cast<ChartCalculationType>()
                .Select(v => new DropdownOption 
                { 
                    Label = v.ToString(), 
                    Value = (int)v 
                })
                .ToList()
        },
        new()
        {
            Title = "نوع گروه‌بندی تاریخ",
            Name = nameof(ChartGroupDateType),
            InputType = "selectbox",
            IsRequired = false,
            Icon = "calendar-group",
            Placeholder = "نوع گروه‌بندی تاریخ را انتخاب کنید",
            IsReadonly = false,
            Disabled = false,
            Options = Enum.GetValues(typeof(ChartGroupDate))
                .Cast<ChartGroupDate>()
                .Select(v => new DropdownOption 
                { 
                    Label = v.ToString(), 
                    Value = (int)v 
                })
                .ToList()
        },

        // Query Configuration
        new()
        {
            Title = "کوئری محور دسته‌بندی",
            Name = nameof(CategoryAxisQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "query",
            Placeholder = "کوئری محور دسته‌بندی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کوئری گروه‌بندی",
            Name = nameof(GroupByQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "group",
            Placeholder = "کوئری گروه‌بندی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کوئری مرتب‌سازی",
            Name = nameof(OrderByQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "sort",
            Placeholder = "کوئری مرتب‌سازی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "شرط",
            Name = nameof(Condition),
            InputType = "textarea",
            IsRequired = false,
            Icon = "filter",
            Placeholder = "شرط اعمالی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Field References
        new()
        {
            Title = "فیلد گروه‌بندی",
            Name = nameof(GroupField),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "group-field",
            Placeholder = "فیلد گروه‌بندی را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "فیلد تاریخ",
            Name = nameof(DateField),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "calendar",
            Placeholder = "فیلد تاریخ را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "فرم ورود اطلاعات",
            Name = nameof(InformationEntryForm),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "form",
            Placeholder = "فرم ورود اطلاعات را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "فیلد محاسباتی",
            Name = nameof(CalculationField),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "calculate",
            Placeholder = "فیلد محاسباتی را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Layout & Display
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
        new()
        {
            Title = "تعداد ستون‌ها",
            Name = nameof(ColumnSpan),
            InputType = "number",
            IsRequired = false,
            Icon = "columns",
            Placeholder = "تعداد ستون‌های اشغالی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "تعداد ردیف‌ها",
            Name = nameof(RowSpan),
            InputType = "number",
            IsRequired = false,
            Icon = "rows",
            Placeholder = "تعداد ردیف‌های اشغالی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "زمان بارگذاری مجدد (ثانیه)",
            Name = nameof(ReloadTime),
            InputType = "number",
            IsRequired = false,
            Icon = "refresh",
            Placeholder = "زمان به ثانیه را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Value Ranges
        new()
        {
            Title = "حداکثر مقدار",
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
            Title = "حداقل مقدار",
            Name = nameof(MinValue),
            InputType = "number",
            IsRequired = false,
            Icon = "min",
            Placeholder = "حداقل مقدار را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "مقدار نشانگر",
            Name = nameof(IndicatorValue),
            InputType = "number",
            IsRequired = false,
            Icon = "indicator",
            Placeholder = "مقدار نشانگر را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Visual Styling
        new()
        {
            Title = "تم",
            Name = nameof(Theme),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "palette",
            Placeholder = "تم را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "آیکون",
            Name = nameof(Icon),
            InputType = "icon-picker",
            IsRequired = false,
            Icon = "image",
            Placeholder = "آیکون را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "رنگ پس‌زمینه",
            Name = nameof(BackgroundColor),
            InputType = "color",
            IsRequired = false,
            Icon = "background",
            Placeholder = "رنگ پس‌زمینه را انتخاب کنید",
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
            Title = "شناسه والد",
            Name = nameof(ParentID),
            InputType = "number",
            IsRequired = false,
            Icon = "parent",
            Placeholder = "شناسه داشبورد والد را وارد کنید",
            IsReadonly = false,
            Disabled = false
        }
    ];
}