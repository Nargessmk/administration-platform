namespace Cloudland.APM.DTO.NodeObjects;

public class InformationEntryFormDTO : ValidatableNodeObjectDTO<Guid>
{
    public required string InformationEntryFormName { get; set; }
    public required string Query { get; set; }
    public required string ConditionQuery { get; set; }
    public required string OrderQuery { get; set; }
    public long RelatedTable { get; set; }
    public long ExternalField { get; set; }
    //public long CoreObjectID { get; set; }
    public long ParentID { get; set; }
    public long ShowRecordCountDefault { get; set; }
    public bool IsDefault { get; set; }
    public bool IsShowID { get; set; }
    public bool ShowSelectedColumn { get; set; }
    public bool SearchWithOnKeyDown { get; set; }
    public long SearchWithOnKeyDownCoreId { get; set; }
    public string? Icon { get; set; }
    public string? BadgeTitle { get; set; }
    public string? BadgeQuery { get; set; }
    public string? BadgeColor { get; set; }
    public string? BadgeTextColor { get; set; }
    public string? CSS { get; set; }
    public bool ShowAttachment { get; set; }
    public bool ShowDetailAttachment { get; set; }
    public bool ShowInParentForm { get; set; }
    public bool ShowInMenuTreeList { get; set; }
    public bool Groupable { get; set; }
    public bool Pageable { get; set; }
    public bool AggregateUsable { get; set; }
    public bool ShowAttachmentColumn { get; set; }
    public long AttachmentColumnName { get; set; }
    public string? TitleSaveButton { get; set; }
    public int HeightAttachment { get; set; }

    public GridEditMode
        GridEditMode { get; set; }

    public bool ShowChartInformationEntryForm { get; set; }
    public bool IsCloseFormAfterSave { get; set; }

    public string? ChartID { get; set; }
    public string? ChartName { get; set; }
    public string? ChartTitle { get; set; }
    public string? ChartGroup { get; set; }
    public string? ChartAvatar { get; set; }
    public string? ChartParentID { get; set; }
    public long RowColorColumnName { get; set; }
    public string? RowColorOperator { get; set; }
    public string? RowColorColumnValue { get; set; }
    public string? RowColorSelectedColor { get; set; }

    public bool ShowLineNumber { get; set; }
    public bool ShowClearFormWithOutFixItemButton { get; set; }
    public string? FixItemClearForm { get; set; }
    public string? ClearFormWithOutFixItemButtonName { get; set; }
    public int RowCountInPage { get; set; }
    public bool SaveParentSubjectSaveChild { get; set; }
    public string? FormComment { get; set; }
    public bool ShowAttachmentButton { get; set; }
    public bool CheckValidChildGrid { get; set; }
    public bool ShowNewButton { get; set; }
    public int Height { get; set; }
    public string? DefaultColumnShowInGrid { get; set; }
    public string? EditorFormJson { get; set; }
    public long GroupableField { get; set; }
    public string? NewPageUrl { get; set; }
    public string? UpdatePageUrl { get; set; }
    public string? NewButtonVisibleConditionQuery { get; set; }
    public string? GroupByQuery { get; set; }
    public bool SaveAtOnce { get; set; }
    public string? CheckFieldDuplicateRecords { get; set; }
    public string? ColumnWidth { get; set; }
    public string? AttachmentColumnWidth { get; set; }
    public string? AnalystDescription { get; set; }
    public bool ShowNewButtonInToolbar { get; set; }
    public bool ShowUpdateButtonInToolbar { get; set; }
    public bool ShowDeleteButtonInToolbar { get; set; }
    public bool ShowAttachmentButtonInToolbar { get; set; }
    public bool ShowViewButtonInToolbar { get; set; }
    public OpenFormType OpenFormType { get; set; }

    public override List<ValidationRule> ValidationRules { get; set; } =
    [
        new()
        {
            Title = "نام فرم",
            Name = nameof(InformationEntryFormName),
            InputType = "text",
            IsRequired = true,
            Icon = "form",
            Placeholder = "نام فرم را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نوع نمایش فرم",
            Name = nameof(OpenFormType),
            InputType = "selectbox",
            IsRequired = true,
            Icon = "display-type",
            Placeholder = "نوع نمایش فرم را انتخاب کنید",
            IsReadonly = false,
            Disabled = false,
            Options = Enum.GetValues(typeof(OpenFormType))
                .Cast<OpenFormType>()
                .Select(v => new DropdownOption 
                { 
                    Label = v.ToString(), 
                    Value = (int)v 
                })
                .ToList()
        },
        new()
        {
            Title = "نوع درج",
            Name = nameof(GridEditMode),
            InputType = "selectbox",
            IsRequired = true,
            Icon = "edit-mode",
            Placeholder = "نوع درج را انتخاب کنید",
            IsReadonly = false,
            Disabled = false,
            Options = Enum.GetValues(typeof(GridEditMode))
                .Cast<GridEditMode>()
                .Select(v => new DropdownOption 
                { 
                    Label = v.ToString(), 
                    Value = (int)v 
                })
                .ToList()
        },

        // Table and Field References
        new()
        {
            Title = "جدول مرتبط",
            Name = nameof(RelatedTable),
            InputType = "dropdown",
            IsRequired = true,
            Icon = "table",
            Placeholder = "جدول مرتبط را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "فیلد مرتبط",
            Name = nameof(ExternalField),
            InputType = "dropdown",
            IsRequired = true,
            Icon = "field-link",
            Placeholder = "فیلد مرتبط را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "فیلد گروهبندی",
            Name = nameof(GroupableField),
            InputType = "dropdown",
            IsRequired = true,
            Icon = "group",
            Placeholder = "فیلد گروهبندی را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Display Settings
        new()
        {
            Title = "تعداد نمایش رکورد",
            Name = nameof(ShowRecordCountDefault),
            InputType = "number",
            IsRequired = false,
            Icon = "row-count",
            Placeholder = "تعداد رکوردهای پیشفرض را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "طول فرم",
            Name = nameof(Height),
            InputType = "number",
            IsRequired = false,
            Icon = "height",
            Placeholder = "طول فرم را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "عرض فرم",
            Name = nameof(ColumnWidth),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "width",
            Placeholder = "عرض فرم را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "توضیحات فرم",
            Name = nameof(FormComment),
            InputType = "text",
            IsRequired = false,
            Icon = "comment",
            Placeholder = "توضیحات فرم را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "عنوان دکمه ذخیره",
            Name = nameof(TitleSaveButton),
            InputType = "text",
            IsRequired = false,
            Icon = "save",
            Placeholder = "عنوان دکمه ذخیره را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
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

        // Form Behavior
        new()
        {
            Title = "نمایش شناسه",
            Name = nameof(IsShowID),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "id",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ذخیره بصورت یکجا",
            Name = nameof(SaveAtOnce),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "save-all",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ذخیره والد مشروط ذخیره فرزند",
            Name = nameof(SaveParentSubjectSaveChild),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "parent-child",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "بررسی فرزند قبل از ذخیره",
            Name = nameof(CheckValidChildGrid),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "validation",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش در فرم والد",
            Name = nameof(ShowInParentForm),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "parent-form",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش در لیست منو",
            Name = nameof(ShowInMenuTreeList),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "menu",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "قابلیت صفحه بندی",
            Name = nameof(Pageable),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "pagination",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "تعداد ردیف در هر صفحه",
            Name = nameof(RowCountInPage),
            InputType = "number",
            IsRequired = false,
            Icon = "rows",
            Placeholder = "تعداد ردیف در هر صفحه را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "قابلیت گروهبندی",
            Name = nameof(Groupable),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "grouping",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش زیر صفحه",
            Name = nameof(AggregateUsable),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "aggregate",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش ستون انتخاب",
            Name = nameof(ShowSelectedColumn),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "select-column",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "جستجو با onKeyDown",
            Name = nameof(SearchWithOnKeyDown),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "search-key",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "فرم جستجو مرتبط",
            Name = nameof(SearchWithOnKeyDown),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "search-form",
            Placeholder = "فرم جستجو را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش ستون ردیف",
            Name = nameof(ShowLineNumber),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "row-number",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "بستن فرم بعد از ذخیره",
            Name = nameof(IsCloseFormAfterSave),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "close",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش دکمه جدید در فرم",
            Name = nameof(ShowNewButton),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "new",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش دکمه پاک کردن فرم به استثناء",
            Name = nameof(ShowClearFormWithOutFixItemButton),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "clear",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "عنوان دکمه پاک کردن فرم به استثناء",
            Name = nameof(ClearFormWithOutFixItemButtonName),
            InputType = "text",
            IsRequired = false,
            Icon = "button-text",
            Placeholder = "عنوان دکمه را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "فیلد های استثناء پاک کردن",
            Name = nameof(FixItemClearForm),
            InputType = "text",
            IsRequired = false,
            Icon = "exclude",
            Placeholder = "فیلدهای استثناء را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ستون پیشفرض نمایش گرید",
            Name = nameof(DefaultColumnShowInGrid),
            InputType = "text",
            IsRequired = false,
            Icon = "column-default",
            Placeholder = "ستون های پیشفرض را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "فیلد های عدم تکرار در ایجاد ردیف",
            Name = nameof(CheckFieldDuplicateRecords),
            InputType = "text",
            IsRequired = false,
            Icon = "unique",
            Placeholder = "فیلدهای منحصر بفرد را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "صفحه دکمه جدید",
            Name = nameof(NewPageUrl),
            InputType = "url",
            IsRequired = false,
            Icon = "new-page",
            Placeholder = "آدرس صفحه جدید را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "صفحه دکمه ویرایش",
            Name = nameof(UpdatePageUrl),
            InputType = "url",
            IsRequired = false,
            Icon = "edit-page",
            Placeholder = "آدرس صفحه ویرایش را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Form Design and Queries
        new()
        {
            Title = "فرم طراحی شده",
            Name = nameof(EditorFormJson),
            InputType = "form-designer",
            IsRequired = false,
            Icon = "design",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کوئری نمایش دکمه جدید",
            Name = nameof(NewButtonVisibleConditionQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "query",
            Placeholder = "کوئری نمایش دکمه جدید را وارد کنید",
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
            Placeholder = "کوئری اصلی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "شرط کوئری",
            Name = nameof(ConditionQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "condition",
            Placeholder = "شرط کوئری را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "گروهبندی کوئری",
            Name = nameof(GroupByQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "group-query",
            Placeholder = "کوئری گروهبندی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ترتیب کوئری",
            Name = nameof(OrderQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "sort",
            Placeholder = "کوئری مرتب سازی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "CSS",
            Name = nameof(CSS),
            InputType = "textarea",
            IsRequired = false,
            Icon = "css",
            Placeholder = "کد CSS را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "توضیحات برنامه نویس/تحلیلگر",
            Name = nameof(AnalystDescription),
            InputType = "textarea",
            IsRequired = false,
            Icon = "developer",
            Placeholder = "توضیحات فنی را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Attachment Settings
        new()
        {
            Title = "نمایش ضمیمه در فرم",
            Name = nameof(ShowAttachment),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "attachment",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "طول ضمیمه",
            Name = nameof(HeightAttachment),
            InputType = "number",
            IsRequired = false,
            Icon = "height",
            Placeholder = "طول بخش ضمیمه را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "عرض ضمیمه",
            Name = nameof(AttachmentColumnWidth),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "width",
            Placeholder = "عرض ستون ضمیمه را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش دکمه ضمیمه",
            Name = nameof(ShowAttachmentButton),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "attachment-button",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش جزئیات ضمیمه در فرم",
            Name = nameof(ShowDetailAttachment),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "attachment-detail",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نمایش ستون ضمیمه",
            Name = nameof(ShowAttachmentColumn),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "attachment-column",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ستون ضمیمه",
            Name = nameof(AttachmentColumnName),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "attachment-field",
            Placeholder = "ستون ضمیمه را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Chart Settings
        new()
        {
            Title = "نمایش بصورت چارت",
            Name = nameof(ShowChartInformationEntryForm),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "chart",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ستون شناسه",
            Name = nameof(ChartID),
            InputType = "text",
            IsRequired = false,
            Icon = "chart-id",
            Placeholder = "ستون شناسه چارت را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ستون نام",
            Name = nameof(ChartName),
            InputType = "text",
            IsRequired = false,
            Icon = "chart-name",
            Placeholder = "ستون نام چارت را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ستون عنوان",
            Name = nameof(ChartTitle),
            InputType = "text",
            IsRequired = false,
            Icon = "chart-title",
            Placeholder = "ستون عنوان چارت را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ستون والد",
            Name = nameof(ChartParentID),
            InputType = "text",
            IsRequired = false,
            Icon = "chart-parent",
            Placeholder = "ستون والد چارت را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ستون گروه",
            Name = nameof(ChartGroup),
            InputType = "text",
            IsRequired = false,
            Icon = "chart-group",
            Placeholder = "ستون گروه چارت را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "ستون تصویر",
            Name = nameof(ChartAvatar),
            InputType = "text",
            IsRequired = false,
            Icon = "chart-image",
            Placeholder = "ستون تصویر چارت را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Badge Settings
        new()
        {
            Title = "عنوان",
            Name = nameof(BadgeTitle),
            InputType = "text",
            IsRequired = false,
            Icon = "badge",
            Placeholder = "عنوان برچسب را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "رنگ زمینه",
            Name = nameof(BadgeColor),
            InputType = "color",
            IsRequired = false,
            Icon = "background",
            Placeholder = "رنگ زمینه برچسب را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "رنگ متن",
            Name = nameof(BadgeTextColor),
            InputType = "color",
            IsRequired = false,
            Icon = "text-color",
            Placeholder = "رنگ متن برچسب را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "کوئری",
            Name = nameof(BadgeQuery),
            InputType = "textarea",
            IsRequired = false,
            Icon = "query",
            Placeholder = "کوئری برچسب را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Row Color Settings
        new()
        {
            Title = "نام ستون",
            Name = nameof(RowColorColumnName),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "column",
            Placeholder = "ستون رنگ سطر را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "نوع عملگر",
            Name = nameof(RowColorOperator),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "operator",
            Placeholder = "عملگر را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "مقدار ستون",
            Name = nameof(RowColorColumnValue),
            InputType = "text",
            IsRequired = false,
            Icon = "value",
            Placeholder = "مقدار مقایسه را وارد کنید",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "رنگ سطر",
            Name = nameof(RowColorSelectedColor),
            InputType = "dropdown",
            IsRequired = false,
            Icon = "color",
            Placeholder = "رنگ سطر را انتخاب کنید",
            IsReadonly = false,
            Disabled = false
        },

        // Toolbar Buttons
        new()
        {
            Title = "دکمه جدید",
            Name = nameof(ShowNewButtonInToolbar),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "new",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "دکمه ضمیمه",
            Name = nameof(ShowAttachmentButtonInToolbar),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "attachment",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "دکمه ویرایش",
            Name = nameof(ShowUpdateButtonInToolbar),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "edit",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "دکمه حذف",
            Name = nameof(ShowDeleteButtonInToolbar),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "delete",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        },
        new()
        {
            Title = "دکمه مشاهده",
            Name = nameof(ShowViewButtonInToolbar),
            InputType = "checkbox",
            IsRequired = false,
            Icon = "view",
            Placeholder = "",
            IsReadonly = false,
            Disabled = false
        }
    ];
}