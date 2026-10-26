namespace Cloudland.APM.DTO.NodeObjects;

public class DisplayFieldDTO : ValidatableNodeObjectDTO<Guid>
{
    public TableButtonEventsType TableButtonEventsType { get; set; }
    public long ReportID { get; set; }
    public long ParameterReportID { get; set; }
    //public long CoreObjectID { get; set; }
    public string? Template { get; set; }
    public string? FullName { get; set; }
    public string? DisplayName { get; set; }
    public bool ShowInStart { get; set; }
    public bool UseDefaultReportSetting { get; set; }
    public string? PrinterName { get; set; }
    public int PrintCopy { get; set; }
    public string? QueryPrintCopy { get; set; }
    public string? ExecutionConditionQuery { get; set; }
    public string? Query { get; set; }
    public string? Icon { get; set; }
    public bool ShowTitle { get; set; }
    public long RelatedForm { get; set; }
    public long RelatedField { get; set; }
    public bool IsReloadGrid { get; set; }

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
            Title = "نام کامل",
            Name = nameof(FullName),
            InputType = "text",
            IsRequired = true,
            Icon = "field",
            Placeholder = "نام کامل فیلد را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نام نمایشی",
            Name = nameof(DisplayName),
            InputType = "text",
            IsRequired = false,
            Icon = "text-field",
            Placeholder = "نام نمایشی فیلد را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش در ابتدای ستون",
            Name = nameof(ShowInStart),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "column-first",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش عنوان",
            Name = nameof(ShowTitle),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "title",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "بروز رسانی داده",
            Name = nameof(IsReloadGrid),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "refresh",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },

        // Action Configuration
        new()
        {
            Title = "نوع عملیات",
            Name = nameof(TableButtonEventsType),
            InputType = "selectbox",
            IsRequired = true,
            Icon = "action",
            Placeholder = "نوع عملیات را انتخاب کنید",
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
            InputType = "dropdown",
            IsRequired = false,
            Icon = "form",
            Placeholder = "فرم مرتبط را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "فیلد مرتبط",
            Name = nameof(RelatedField),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "field-link",
            Placeholder = "فیلد مرتبط را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Report Settings
        new()
        {
            Title = "گزارش",
            Name = nameof(ReportID),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "report",
            Placeholder = "گزارش را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "پارامتر گزارش",
            Name = nameof(ParameterReportID),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "report-parameter",
            Placeholder = "پارامتر گزارش را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Template and Queries
        new()
        {
            Title = "دستور",
            Name = nameof(Template),
            InputType = "textarea",
            IsRequired = true,
            Icon = "code",
            Placeholder = "دستور نمایش را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کوئری",
            Name = nameof(Query),
            InputType = "textarea",
            IsRequired = false,
            Icon = "query",
            Placeholder = "کوئری را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "شرط اجرای کوئری",
            Name = nameof(ExecutionConditionQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "conditional",
            Placeholder = "شرط اجرای کوئری را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Visual Settings
        new()
        {
            Title = "آیکن",
            Name = nameof(Icon),
            InputType = "icon-picker",
            IsRequired = false,
            Icon = "image",
            Placeholder = "آیکن را انتخاب کنید",
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

        // Email Settings
        new()
        {
            Title = "استفاده از ایمیل پیشفرض",
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
            Title = "نام کاربری",
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
            Title = "کلمه عبور",
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
            Title = "سرور",
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
            Title = "پورت",
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
            Title = "فعال کردن SSL",
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
            Title = "کاربران دریافت کننده",
            Name = nameof(ReceivingUsers),
            InputType = "multiselect",
            IsRequired = false,
            Icon = "users",
            Placeholder = "کاربران دریافت کننده را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نقش کاربر دریافت کننده",
            Name = nameof(ReceivingRole),
            InputType = "multiselect",
            IsRequired = false,
            Icon = "roles",
            Placeholder = "نقش های دریافت کننده را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کاربر درج کننده",
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
            Title = "ارسال پیوست",
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
            Title = "ارسال گزارش",
            Name = nameof(SendReport),
            InputType = "multiselect",
            IsRequired = false,
            Icon = "report-send",
            Placeholder = "گزارش های ارسالی را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "عنوان",
            Name = nameof(Title),
            InputType = "text",
            IsRequired = false,
            Icon = "title",
            Placeholder = "عنوان ایمیل را وارد کنید",
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
            Placeholder = "کوئری عنوان ایمیل را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "متن",
            Name = nameof(BodyMessage),
            InputType = "textarea",
            IsRequired = false,
            Icon = "message",
            Placeholder = "متن ایمیل را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کوئری متن",
            Name = nameof(BodyMessageQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "query",
            Placeholder = "کوئری متن ایمیل را وارد کنید",
            IsReadonly = false,
            Disabled = false
        }
    ];
}