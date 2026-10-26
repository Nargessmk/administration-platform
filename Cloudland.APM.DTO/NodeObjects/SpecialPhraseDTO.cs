namespace Cloudland.APM.DTO.NodeObjects;

public class SpecialPhraseDTO : ValidatableNodeObjectDTO<Guid>
{
    public required string Query { get; set; }
    //public long CoreObjectID { get; set; }
    public long DataSourceID { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "پایگاه داده مرتبط",
            Name = nameof(DataSourceID),
            InputType = "selectbox",
            IsRequired = false,
            Icon = "database-link",
            Placeholder = "پایگاه داده را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Query Configuration
        new()
        {
            Title = "کوئری",
            Name = nameof(Query),
            InputType = "textarea",
            IsRequired = false,
            Icon = "sql-query",
            Placeholder = "کوئری SQL را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // System Reference (hidden/readonly)
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