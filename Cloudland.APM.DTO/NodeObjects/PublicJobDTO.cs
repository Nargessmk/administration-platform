namespace Cloudland.APM.DTO.NodeObjects;

public class PublicJobDTO : ValidatableNodeObjectDTO<Guid>
{
    public required string FullName { get; set; }
    public required string Query { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    //public long CoreObjectID { get; set; }
    public long RelatedDatasource { get; set; }
    public int RepeatDay { get; set; }
    public int RepeatClock { get; set; }
    public PublicJobType PublicJobType { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نام کار",
            Name = nameof(FullName),
            InputType = "text",
            IsRequired = true,
            Icon = "job",
            Placeholder = "نام کار را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Job Configuration
        new()
        {
            Title = "نوع عملیات",
            Name = nameof(PublicJobType),
            InputType = "selectbox",
            IsRequired = true,
            Icon = "job-type",
            Placeholder = "نوع عملیات را انتخاب کنید",
            IsReadonly = false,
            Disabled = false,
            Options = Enum.GetValues(typeof(PublicJobType))
                .Cast<PublicJobType>()
                .Select(v => new DropdownOption 
                { 
                    Label = v.ToString(), 
                    Value = (int)v 
                })
                .ToList()
        },
        new()
        {
            Title = "منبع مرتبط",
            Name = nameof(RelatedDatasource),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "database",
            Placeholder = "منبع داده را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Scheduling
        new()
        {
            Title = "تاریخ شروع",
            Name = nameof(StartDate),
            InputType = "date",
            IsRequired = false,
            Icon = "calendar-start",
            Placeholder = "تاریخ شروع را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "تاریخ پایان",
            Name = nameof(EndDate),
            InputType = "date",
            IsRequired = false,
            Icon = "calendar-end",
            Placeholder = "تاریخ پایان را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ساعت شروع",
            Name = nameof(StartTime),
            InputType = "time",
            IsRequired = false,
            Icon = "clock-start",
            Placeholder = "ساعت شروع را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ساعت پایان",
            Name = nameof(EndTime),
            InputType = "time",
            IsRequired = false,
            Icon = "clock-end",
            Placeholder = "ساعت پایان را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "تکرار روز",
            Name = nameof(RepeatDay),
            InputType = "number",
            IsRequired = false,
            Icon = "repeat-day",
            Placeholder = "تعداد روزهای تکرار را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "تکرار ساعت",
            Name = nameof(RepeatClock),
            InputType = "number",
            IsRequired = false,
            Icon = "repeat-hour",
            Placeholder = "تعداد ساعات تکرار را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Query
        new()
        {
            Title = "کوئری",
            Name = nameof(Query),
            InputType = "textarea",
            IsRequired = false,
            Icon = "query",
            Placeholder = "کوئری اجرایی را وارد کنید",
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