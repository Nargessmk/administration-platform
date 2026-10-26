namespace Cloudland.APM.DTO.NodeObjects;

public class DashboardIntegrationDTO : ValidatableNodeObjectDTO<Guid>
{
    public required string FullName { get; set; }
    //public long CoreObjectID { get; set; }
    public long GroupField { get; set; }
    public ChartCalculationType ChartCalculationType { get; set; }
    public required string CategoryAxisQuery { get; set; }
    public required string Condition { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نام یکپارچه‌سازی",
            Name = nameof(FullName),
            InputType = "text",
            IsRequired = true,
            Icon = "integration",
            Placeholder = "نام یکپارچه‌سازی داشبورد را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Core Object Reference
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

        // Grouping and Calculation
        new()
        {
            Title = "فیلد گروه‌بندی",
            Name = nameof(GroupField),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "group",
            Placeholder = "فیلد گروه‌بندی را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Chart Configuration
        new()
        {
            Title = "نوع محاسبه نمودار",
            Name = nameof(ChartCalculationType),
            InputType = "dropdown",
            IsRequired = true,
            Icon = "chart",
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
            Title = "شرط",
            Name = nameof(Condition),
            InputType = "textarea",
            IsRequired = false,
            Icon = "filter",
            Placeholder = "شرط اعمالی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        }
    ];
}