namespace Cloudland.APM.Models;

public class TableModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } // توضیح جدول (در صورت استفاده از Extended Properties)
    public List<ColumnModel> Columns { get; set; } = new();
    public List<ForeignKeyModel> ForeignKeys { get; set; } = new();

    public List<IndexModel> Indexes { get; set; } = new();

    // می‌توان کلید اولیه یا سایر قیود را نیز نگهداری کرد:
    public List<string> PrimaryKeyColumns { get; set; } = new(); // ستون‌های کلید اصلی
    public List<CheckConstraintModel> CheckConstraints { get; set; } = new();
}