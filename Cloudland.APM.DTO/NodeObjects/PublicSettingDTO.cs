namespace Cloudland.APM.DTO.NodeObjects;

public class PublicSettingDTO : ValidatableNodeObjectDTO<Guid>
{
    public string? CompanyName { get; set; }
    public string? AppPersianName { get; set; }
    public string? AppEnglishName { get; set; }
    public string? WebSite { get; set; }
    public string? PhoneNumber { get; set; }
    public string? E_Maile { get; set; }
    public string? EMaileUserName { get; set; }
    public string? EMailePassWord { get; set; }
    public string? EMaileServer { get; set; }
    public string? EMailePort { get; set; }
    public bool EnableSsl { get; set; }

    public string? BrandSlogan { get; set; }
    public string? AppLogo { get; set; }
    public string? MainColor { get; set; }
    public string? IconColor { get; set; }
    public string? TitleColor { get; set; }
    public string? IntroduceOthers { get; set; }
    public string? Laws { get; set; }
    public string? UserPrivacy { get; set; }
    public string? FileSavingPath { get; set; }
    public bool SaveFileValueInDataBase { get; set; }
    public string? LoginBackgroundImage { get; set; }
    public string? HeaderLoginBackgroundImage { get; set; }
    public string? HomeBackgroundImage { get; set; }
    public string? GeneralMessage { get; set; }
    public string? CSS { get; set; }
    public string? JS { get; set; }
    public long RelatedWebService { get; set; }
    public long ChangePasswordDays { get; set; }
    public string? SupportLink { get; set; }
    public int DataRecoveryMinutes { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نام شرکت",
            Name = nameof(CompanyName),
            InputType = "text",
            IsRequired = true,
            Icon = "company",
            Placeholder = "نام شرکت را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نام فارسی اپلیکیشن",
            Name = nameof(AppPersianName),
            InputType = "text",
            IsRequired = true,
            Icon = "app",
            Placeholder = "نام فارسی برنامه را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نام انگلیسی اپلیکیشن",
            Name = nameof(AppEnglishName),
            InputType = "text",
            IsRequired = true,
            Icon = "app",
            Placeholder = "نام انگلیسی برنامه را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "شعار برند",
            Name = nameof(BrandSlogan),
            InputType = "text",
            IsRequired = true,
            Icon = "slogan",
            Placeholder = "شعار برند را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Contact Information
        new()
        {
            Title = "وبسایت",
            Name = nameof(WebSite),
            InputType = "url",
            IsRequired = false,
            Icon = "website",
            Placeholder = "آدرس وبسایت را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "شماره تماس",
            Name = nameof(PhoneNumber),
            InputType = "tel",
            IsRequired = false,
            Icon = "phone",
            Placeholder = "شماره تماس را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ایمیل",
            Name = nameof(E_Maile),
            InputType = "email",
            IsRequired = false,
            Icon = "email",
            Placeholder = "آدرس ایمیل را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "لینک پشتیبان",
            Name = nameof(SupportLink),
            InputType = "url",
            IsRequired = false,
            Icon = "support",
            Placeholder = "لینک پشتیبانی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Visual Settings
        new()
        {
            Title = "رنگ پس زمینه",
            Name = nameof(MainColor),
            InputType = "color",
            IsRequired = true,
            Icon = "background",
            Placeholder = "رنگ اصلی را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "رنگ آیکن",
            Name = nameof(IconColor),
            InputType = "color",
            IsRequired = true,
            Icon = "icon",
            Placeholder = "رنگ آیکن ها را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "رنگ حروف",
            Name = nameof(TitleColor),
            InputType = "color",
            IsRequired = true,
            Icon = "text",
            Placeholder = "رنگ متن ها را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "لوگوی شرکت",
            Name = nameof(AppLogo),
            InputType = "image",
            IsRequired = true,
            Icon = "logo",
            Placeholder = "لوگو را آپلود کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "تصویر زمینه ورودی",
            Name = nameof(LoginBackgroundImage),
            InputType = "image",
            IsRequired = true,
            Icon = "login-bg",
            Placeholder = "تصویر زمینه صفحه ورود را آپلود کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "تصویر زمینه خانه",
            Name = nameof(HomeBackgroundImage),
            InputType = "image",
            IsRequired = true,
            Icon = "home-bg",
            Placeholder = "تصویر زمینه صفحه اصلی را آپلود کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Content Settings
        new()
        {
            Title = "معرفی به دیگران",
            Name = nameof(IntroduceOthers),
            InputType = "text",
            IsRequired = true,
            Icon = "share",
            Placeholder = "متن معرفی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "پیغام عمومی",
            Name = nameof(GeneralMessage),
            InputType = "text",
            IsRequired = true,
            Icon = "message",
            Placeholder = "پیغام عمومی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "قوانین",
            Name = nameof(Laws),
            InputType = "textarea",
            IsRequired = true,
            Icon = "law",
            Placeholder = "قوانین را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "حریم خصوصی",
            Name = nameof(UserPrivacy),
            InputType = "textarea",
            IsRequired = true,
            Icon = "privacy",
            Placeholder = "متن حریم خصوصی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Technical Settings
        new()
        {
            Title = "مسیر ذخیره ی ضمائم",
            Name = nameof(FileSavingPath),
            InputType = "text",
            IsRequired = true,
            Icon = "folder",
            Placeholder = "مسیر ذخیره فایل ها را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "CSS",
            Name = nameof(CSS),
            InputType = "textarea",
            IsRequired = true,
            Icon = "css",
            Placeholder = "کدهای CSS را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "JS",
            Name = nameof(JS),
            InputType = "textarea",
            IsRequired = true,
            Icon = "javascript",
            Placeholder = "کدهای JavaScript را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Email Settings
        new()
        {
            Title = "نام کاربری ایمیل",
            Name = nameof(EMaileUserName),
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
            Name = nameof(EMailePassWord),
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
            Name = nameof(EMaileServer),
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
            Name = nameof(EMailePort),
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

        // Security Settings
        new()
        {
            Title = "وب سرویس مرتبط",
            Name = nameof(RelatedWebService),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "web-service",
            Placeholder = "وب سرویس پیامک را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "تعداد روزهای تغییر کلمه عبور",
            Name = nameof(ChangePasswordDays),
            InputType = "number",
            IsRequired = false,
            Icon = "password-change",
            Placeholder = "تعداد روزهای تغییر رمز را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "زمان بازیابی داده ها(دقیقه)",
            Name = nameof(DataRecoveryMinutes),
            InputType = "number",
            IsRequired = false,
            Icon = "recovery",
            Placeholder = "زمان بازیابی داده را وارد کنید",
            IsReadonly = false,
            Disabled = false
        }
    ];
}