namespace Cloudland.APM.DTO.NodeObjects;

public class ButtonDTO : ValidatableNodeObjectDTO<Guid>
{
    public required string FullName { get; set; }
    public required string Icon { get; set; }
    public required string Query { get; set; }
    public required string ExecutionConditionQuery { get; set; }
    public TableButtonEventsType TableButtonEventsType { get; set; }
    public long CoreObjectID { get; set; }
    public long RelatedForm { get; set; }
    public long RelatedField { get; set; }
    public long RelatedWebService { get; set; }
    public bool IsReloadGrid { get; set; }
    public bool IsShowText { get; set; }
    public bool IsShowIcon { get; set; }

    public string? EMail { get; set; }
    public string? EMailUserName { get; set; }
    public string? EMailPassWord { get; set; }
    public string? EMailServer { get; set; }
    public string? EMailPort { get; set; }
    public string[]? ReceivingUsers { get; set; }
    public string[]? ReceivingRole { get; set; }
    public bool InsertingUser { get; set; }
    public string? ReceivingQuery { get; set; }
    public bool SendAttachmentFile { get; set; }
    public bool EnableSsl { get; set; }
    public bool? UsePublicEmail { get; set; }
    public string[]? SendReport { get; set; }
    public string? Title { get; set; }
    public string? TitleQuery { get; set; }
    public string? BodyMessage { get; set; }
    public string? BodyMessageQuery { get; set; }
    public string? URL { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نام دکمه",
            Name = nameof(FullName),
            InputType = "text",
            IsRequired = true,
            Icon = "button",
            Placeholder = "نام دکمه را وارد کنید",
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
            Title = "نمایش متن",
            Name = nameof(IsShowText),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "text",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش آیکون",
            Name = nameof(IsShowIcon),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "image",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "بارگذاری مجدد گرید",
            Name = nameof(IsReloadGrid),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "refresh",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },

        // Button Action Properties
        new()
        {
            Title = "نوع رویداد",
            Name = nameof(TableButtonEventsType),
            InputType = "dropdown",
            IsRequired = true,
            Icon = "event",
            Placeholder = "نوع رویداد را انتخاب کنید",
            IsReadonly = false,
            Disabled = false,
            Options = Enum.GetValues(typeof(TableButtonEventsType))
                .Cast<TableButtonEventsType>()
                .Select(v => new DropdownOption 
                { 
                    Label = v.ToString(), 
                    Value = (int)v 
                })
                .ToList()
        },
        new()
        {
            Title = "فرم مرتبط",
            Name = nameof(RelatedForm),
            InputType = "number",
            IsRequired = false,
            Icon = "form",
            Placeholder = "شناسه فرم را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "فیلد مرتبط",
            Name = nameof(RelatedField),
            InputType = "number",
            IsRequired = false,
            Icon = "field",
            Placeholder = "شناسه فیلد را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "وب سرویس مرتبط",
            Name = nameof(RelatedWebService),
            InputType = "number",
            IsRequired = false,
            Icon = "web-service",
            Placeholder = "شناسه وب سرویس را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "URL",
            Name = nameof(URL),
            InputType = "url",
            IsRequired = false,
            Icon = "link",
            Placeholder = "آدرس URL را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Query Properties
        new()
        {
            Title = "کوئری",
            Name = nameof(Query),
            InputType = "textarea",
            IsRequired = false,
            Icon = "code",
            Placeholder = "کوئری را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کوئری شرط اجرا",
            Name = nameof(ExecutionConditionQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "conditional",
            Placeholder = "شرط اجرا را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Email Properties
        new()
        {
            Title = "ایمیل",
            Name = nameof(EMail),
            InputType = "email",
            IsRequired = false,
            Icon = "email",
            Placeholder = "آدرس ایمیل را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نام کاربری ایمیل",
            Name = nameof(EMailUserName),
            InputType = "text",
            IsRequired = false,
            Icon = "user",
            Placeholder = "نام کاربری ایمیل را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کلمه عبور ایمیل",
            Name = nameof(EMailPassWord),
            InputType = "password",
            IsRequired = false,
            Icon = "password",
            Placeholder = "کلمه عبور ایمیل را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "سرور ایمیل",
            Name = nameof(EMailServer),
            InputType = "text",
            IsRequired = false,
            Icon = "server",
            Placeholder = "آدرس سرور ایمیل را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "پورت ایمیل",
            Name = nameof(EMailPort),
            InputType = "text",
            IsRequired = false,
            Icon = "port",
            Placeholder = "پورت ایمیل را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کاربران دریافت کننده",
            Name = nameof(ReceivingUsers),
            InputType = "multiselect",
            IsRequired = false,
            Icon = "users",
            Placeholder = "کاربران را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نقش های دریافت کننده",
            Name = nameof(ReceivingRole),
            InputType = "multiselect",
            IsRequired = false,
            Icon = "roles",
            Placeholder = "نقش ها را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "درج کننده کاربر",
            Name = nameof(InsertingUser),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "user-add",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کوئری دریافت کنندگان",
            Name = nameof(ReceivingQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "query",
            Placeholder = "کوئری دریافت کنندگان را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ارسال فایل پیوست",
            Name = nameof(SendAttachmentFile),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "attachment",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "استفاده از ایمیل عمومی",
            Name = nameof(UsePublicEmail),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "email-public",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "فعالسازی SSL",
            Name = nameof(EnableSsl),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "security",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ارسال گزارش",
            Name = nameof(SendReport),
            InputType = "multiselect",
            IsRequired = false,
            Icon = "report",
            Placeholder = "گزارش ها را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Message Properties
        new()
        {
            Title = "عنوان",
            Name = nameof(Title),
            InputType = "text",
            IsRequired = false,
            Icon = "title",
            Placeholder = "عنوان را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کوئری عنوان",
            Name = nameof(TitleQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "query",
            Placeholder = "کوئری عنوان را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "متن پیام",
            Name = nameof(BodyMessage),
            InputType = "textarea",
            IsRequired = false,
            Icon = "message",
            Placeholder = "متن پیام را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کوئری متن پیام",
            Name = nameof(BodyMessageQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "query",
            Placeholder = "کوئری متن پیام را وارد کنید",
            IsReadonly = false,
            Disabled = false
        }
    ];
}