namespace Cloudland.APM.DTO.NodeObjects;

public class DataSourceInfoDTO : ValidatableNodeObjectDTO<Guid>
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required string DataBase { get; set; }
    public required string ServerName { get; set; }
    public required string FilePath { get; set; }
    public DataSourceType DataSourceType { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نوع منبع داده",
            Name = nameof(DataSourceType),
            InputType = "selectbox",
            IsRequired = true,
            Icon = "database-type",
            Placeholder = "نوع منبع داده را انتخاب کنید",
            IsReadonly = false,
            Disabled = false,
            Options = Enum.GetValues(typeof(DataSourceType))
                .Cast<DataSourceType>()
                .Select(v => new DropdownOption 
                { 
                    Label = v.ToString(), 
                    Value = (int)v 
                })
                .ToList()
        },

        // Database Connection Fields
        new()
        {
            Title = "نام سرور",
            Name = nameof(ServerName),
            InputType = "text",
            IsRequired = false,
            Icon = "server",
            Placeholder = "نام یا آدرس سرور را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نام پایگاه داده",
            Name = nameof(DataBase),
            InputType = "text",
            IsRequired = false,
            Icon = "database",
            Placeholder = "نام پایگاه داده را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نام کاربری",
            Name = nameof(UserName),
            InputType = "text",
            IsRequired = false,
            Icon = "user",
            Placeholder = "نام کاربری را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کلمه عبور",
            Name = nameof(Password),
            InputType = "password",
            IsRequired = false,
            Icon = "password",
            Placeholder = "کلمه عبور را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // File Path (for file-based data sources)
        new()
        {
            Title = "مسیر فایل",
            Name = nameof(FilePath),
            InputType = "file-path",
            IsRequired = false,
            Icon = "folder",
            Placeholder = "مسیر فایل داده را وارد کنید",
            IsReadonly = false,
            Disabled = false
        }
    ];
}