namespace Cloudland.APM.DTO.NodeObjects;

public class TableAttachmentDTO : ValidatableNodeObjectDTO<Guid>
{
    public required string FullName { get; set; }
    public string[]? AllowedExtensions { get; set; }
    public AttachmentUploadType AttachmentUploadType { get; set; }
    public AttachmentUploadSize AttachmentUploadSize { get; set; }
    public long CoreObjectID { get; set; }
    public long MaxFileSize { get; set; }
    public long MinFileSize { get; set; }
    public bool IsRequired { get; set; }
    public bool ShowDefault { get; set; }
    public bool SaveInDatabase { get; set; }
    public string? AutoFillQuery { get; set; }
    public string? ColumnWidth { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نام پیوست",
            Name = nameof(FullName),
            InputType = "text",
            IsRequired = true,
            Icon = "attachment",
            Placeholder = "نام پیوست را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Upload Configuration
        new()
        {
            Title = "نوع بارگذاری ضمیمه",
            Name = nameof(AttachmentUploadType),
            InputType = "selectbox",
            IsRequired = true,
            Icon = "upload-type",
            Placeholder = "نوع بارگذاری را انتخاب کنید",
            IsReadonly = false,
            Disabled = false,
            Options = Enum.GetValues(typeof(AttachmentUploadType))
                .Cast<AttachmentUploadType>()
                .Select(v => new DropdownOption 
                { 
                    Label = v.ToString(), 
                    Value = (int)v 
                })
                .ToList()
        },
        new()
        {
            Title = "اندازه ضمیمه",
            Name = nameof(AttachmentUploadSize),
            InputType = "selectbox",
            IsRequired = true,
            Icon = "upload-size",
            Placeholder = "اندازه ضمیمه را انتخاب کنید",
            IsReadonly = false,
            Disabled = false,
            Options = Enum.GetValues(typeof(AttachmentUploadSize))
                .Cast<AttachmentUploadSize>()
                .Select(v => new DropdownOption 
                { 
                    Label = v.ToString(), 
                    Value = (int)v 
                })
                .ToList()
        },

        // File Restrictions
        new()
        {
            Title = "پسوندهای مجاز",
            Name = nameof(AllowedExtensions),
            InputType = "multiselect",
            IsRequired = true,
            Icon = "file-extensions",
            Placeholder = "پسوندهای مجاز را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "حداکثر حجم فایل (KB)",
            Name = nameof(MaxFileSize),
            InputType = "number",
            IsRequired = true,
            Icon = "file-size-max",
            Placeholder = "حداکثر حجم را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "حداقل حجم فایل (KB)",
            Name = nameof(MinFileSize),
            InputType = "number",
            IsRequired = true,
            Icon = "file-size-min",
            Placeholder = "حداقل حجم را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Display Settings
        new()
        {
            Title = "عرض ستون",
            Name = nameof(ColumnWidth),
            InputType = "selectbox",
            IsRequired = true,
            Icon = "column-width",
            Placeholder = "عرض ستون را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Behavior Settings
        new()
        {
            Title = "ضروری",
            Name = nameof(IsRequired),
            InputType = "switch",
            IsRequired = false,
            Icon = "required",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش پیشفرض",
            Name = nameof(ShowDefault),
            InputType = "switch",
            IsRequired = false,
            Icon = "eye",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ذخیره در پایگاه داده",
            Name = nameof(SaveInDatabase),
            InputType = "switch",
            IsRequired = false,
            Icon = "database-save",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },

        // Advanced Settings
        new()
        {
            Title = "دستور پر کردن خودکار",
            Name = nameof(AutoFillQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "auto-fill",
            Placeholder = "دستور SQL را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // System Reference
        new()
        {
            Title = "شناسه شیء اصلی",
            Name = nameof(CoreObjectID),
            InputType = "number",
            IsRequired = true,
            Icon = "object-id",
            Placeholder = "",
            IsReadonly = true,
            Disabled = true
        }
    ];
}