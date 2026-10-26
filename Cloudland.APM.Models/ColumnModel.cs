namespace Cloudland.APM.Models;

public class ColumnModel
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty; // نوع داده (مثلاً "INT" یا "VARCHAR(100)")
    public bool IsNullable { get; set; }
    public bool IsComputed { get; set; }
    public string? ComputedFormula { get; set; } // فرمول محاسباتی در صورت computed column
    public string? DefaultValue { get; set; } // مقدار پیش‌فرض (در صورت وجود)
    public string? Description { get; set; } // توضیح ستون (متن MS_Description)
}