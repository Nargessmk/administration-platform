namespace Cloudland.APM.DTO.NodeObjects;

public class TableFunctionDTO : ValidatableNodeObjectDTO<Guid>
{
    public required string FullName { get; set; }
    public required string ReturnDataType { get; set; } // نوع داده خروجی (int...)
    public required string Query { get; set; } // query تابع
    //public long CoreObjectID { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نام تابع",
            Name = nameof(FullName),
            InputType = "text",
            IsRequired = true,
            Icon = "function",
            Placeholder = "نام تابع را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Function Configuration
        new()
        {
            Title = "نوع داده خروجی",
            Name = nameof(ReturnDataType),
            InputType = "text",
            IsRequired = true,
            Icon = "data-type",
            Placeholder = "نوع خروجی را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Query Definition
        new()
        {
            Title = "کوئری تابع",
            Name = nameof(Query),
            InputType = "textarea",
            IsRequired = true,
            Icon = "sql-query",
            Placeholder = "کوئری SQL تابع را وارد کنید",
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
        //     Placeholder = "",
        //     IsReadonly = true,
        //     Disabled = true
        // }
    ];
}