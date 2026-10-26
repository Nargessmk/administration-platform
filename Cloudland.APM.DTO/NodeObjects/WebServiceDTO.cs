namespace Cloudland.APM.DTO.NodeObjects;

public class WebServiceDTO : ValidatableNodeObjectDTO<Guid>
{
    public required string Name { get; set; }
    public required string URL { get; set; }
    public string? ExportType { get; set; } //data types

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نام وب سرویس",
            Name = nameof(Name),
            InputType = "text",
            IsRequired = true,
            Icon = "webservice",
            Placeholder = "نام وب سرویس را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Connection Settings
        new()
        {
            Title = "آدرس وب سرویس",
            Name = nameof(URL),
            InputType = "text",
            IsRequired = true,
            Icon = "url",
            Placeholder = "آدرس کامل وب سرویس را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Output Configuration
        new()
        {
            Title = "نوع خروجی",
            Name = nameof(ExportType),
            InputType = "selectbox",
            IsRequired = true,
            Icon = "data-type",
            Placeholder = "نوع داده خروجی را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        }
    ];
}