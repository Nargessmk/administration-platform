namespace Cloudland.APM.DTO.NodeObjects;

public class TableEventDTO : ValidatableNodeObjectDTO<Guid>
{
    public string? TypeEventExecution { get; set; } //نوع اجرای رویداد
    public string? EventType { get; set; } // نوع رویداد
    public string? Condition { get; set; } // شرط اجرای رویداد
    public string? Query { get; set; }
    public long RelatedTable { get; set; } // جدول مرتبط
    public long RelatedWebService { get; set; } //تنظیمات ایمیل
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
    public string[]? ReferralRecipientsUser { get; set; }
    public string[]? ReferralRecipientsRole { get; set; }
    public string? ReferralRecipientsQuery { get; set; }
    public int ReferralDeadlineResponse { get; set; }
    public string? ReferralTitle { get; set; }
    public string? ReferralTitleQuery { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نوع اجرای رویداد",
            Name = nameof(TypeEventExecution),
            InputType = "text",
            IsRequired = true,
            Icon = "event-execution",
            Placeholder = "نوع اجرا را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نوع رویداد",
            Name = nameof(EventType),
            InputType = "text",
            IsRequired = true,
            Icon = "event-type",
            Placeholder = "نوع رویداد را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "جدول مرتبط",
            Name = nameof(RelatedTable),
            InputType = "text",
            IsRequired = true,
            Icon = "table-link",
            Placeholder = "جدول مرتبط را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "وب سرویس مرتبط",
            Name = nameof(RelatedWebService),
            InputType = "selectbox",
            IsRequired = true,
            Icon = "webservice",
            Placeholder = "وب سرویس را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "شرط اجرای کوئری",
            Name = nameof(Condition),
            InputType = "textarea",
            IsRequired = false,
            Icon = "condition",
            Placeholder = "شرط SQL را وارد کنید",
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
            Placeholder = "کوئری SQL را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Email Settings
        new()
        {
            Title = "استفاده از ایمیل پیشفرض",
            Name = nameof(UsePublicEmail),
            InputType = "switch",
            IsRequired = false,
            Icon = "email-default",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ایمیل",
            Name = nameof(EMail),
            InputType = "text",
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
            Icon = "username",
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
            Placeholder = "رمز عبور ایمیل را وارد کنید",
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
            InputType = "switch",
            IsRequired = false,
            Icon = "ssl",
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
            Placeholder = "کاربران را انتخاب کنید",
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
            Placeholder = "نقش ها را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کاربر درج کننده",
            Name = nameof(InsertingUser),
            InputType = "switch",
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
            Icon = "query-users",
            Placeholder = "کوئری دریافت کنندگان را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ارسال پیوست",
            Name = nameof(SendAttachmentFile),
            InputType = "switch",
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
            Icon = "report",
            Placeholder = "گزارش ها را انتخاب کنید",
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
            Icon = "query-title",
            Placeholder = "کوئری عنوان را وارد کنید",
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
            Icon = "query-message",
            Placeholder = "کوئری متن را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Referral Settings
        new()
        {
            Title = "مهلت پاسخگویی (روز)",
            Name = nameof(ReferralDeadlineResponse),
            InputType = "number",
            IsRequired = false,
            Icon = "deadline",
            Placeholder = "تعداد روز را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کاربران دریافت کننده",
            Name = nameof(ReferralRecipientsUser),
            InputType = "multiselect",
            IsRequired = false,
            Icon = "users",
            Placeholder = "کاربران را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نقش کاربر دریافت کننده",
            Name = nameof(ReferralRecipientsRole),
            InputType = "multiselect",
            IsRequired = false,
            Icon = "roles",
            Placeholder = "نقش ها را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کوئری دریافت کنندگان",
            Name = nameof(ReferralRecipientsQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "query-users",
            Placeholder = "کوئری دریافت کنندگان را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "دستور ارجاع",
            Name = nameof(ReferralTitle),
            InputType = "textarea",
            IsRequired = false,
            Icon = "referral",
            Placeholder = "دستور ارجاع را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کوئری دستور ارجاع",
            Name = nameof(ReferralTitleQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "query-referral",
            Placeholder = "کوئری دستور ارجاع را وارد کنید",
            IsReadonly = false,
            Disabled = false
        }
    ];
}