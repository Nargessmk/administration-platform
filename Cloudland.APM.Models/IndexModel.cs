namespace Cloudland.APM.Models;

public class IndexModel
{
    public string Name { get; set; } = string.Empty;
    public List<string> KeyColumns { get; set; } = new();
    public List<string> IncludedColumns { get; set; } = new();
    public bool IsUnique { get; set; } = false;
    public bool IsClustered { get; set; } = false;
    public string? FilterCondition { get; set; } // شرط فیلتر برای ایندکس‌های فیلترشده (WHERE ...)
}