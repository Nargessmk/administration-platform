namespace Cloudland.APM.DTO.NodeObjects;

public class DatabaseDTO : ValidatableNodeObjectDTO<Guid>
{
    public required string Source { get; set; }
    public required string DataBase { get; set; }
    public required string DBPassword { get; set; }
    public required string DBUser { get; set; }
    public SQLVersions versions { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "منبع داده",
            Name = nameof(Source),
            InputType = "text",
            IsRequired = false,
            Icon = "database",
            Placeholder = "آدرس سرور یا منبع داده را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نام پایگاه داده",
            Name = nameof(DataBase),
            InputType = "text",
            IsRequired = true,
            Icon = "database",
            Placeholder = "نام پایگاه داده را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "رمز عبور",
            Name = nameof(DBPassword),
            InputType = "password",
            IsRequired = false,
            Icon = "lock",
            Placeholder = "رمز عبور پایگاه داده را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نام کاربری",
            Name = nameof(DBUser),
            InputType = "text",
            IsRequired = true,
            Icon = "user",
            Placeholder = "نام کاربری پایگاه داده را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نسخه SQL",
            Name = nameof(versions),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "version",
            Placeholder = "نسخه SQL Server را انتخاب کنید",
            IsReadonly = false,
            Disabled = false,
            Options = Enum.GetValues(typeof(SQLVersions))
                .Cast<SQLVersions>()
                .Select(v => new DropdownOption 
                { 
                    Label = v.ToString(), 
                    Value = (int)v 
                })
                .ToList()
        }
    ];
}