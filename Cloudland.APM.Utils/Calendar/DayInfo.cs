namespace Cloudland.APM.Utils.Calendar;

/// <summary>
/// مدل خروجی روزهای تقویم
/// </summary>
public class DayInfo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Date { get; set; } // فرمت 1405/05/01
    public required string Title { get; set; }
    public string? Occasion { get; set; }
    public bool IsThursday { get; set; }
    public bool IsFriday { get; set; }
    /// <summary>
    /// آیا تعطیل رسمی است؟ (شامل جمعه‌ها و تعطیلات مناسبتی)
    /// </summary>
    public bool IsHoliday { get; set; }
    
    // جهت دیباگ راحت‌تر
    public override string ToString() => $"{Date} - {Title} (Holiday: {IsHoliday})";
}