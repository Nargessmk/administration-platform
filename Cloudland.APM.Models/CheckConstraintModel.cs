namespace Cloudland.APM.Models;

public class CheckConstraintModel
{
    /// <summary>
    ///     نام قید Check (به عنوان مثال "CHK_TableName_ColumnName")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     عبارت شرطی که باید رعایت شود (مثلاً "Age > 18")
    /// </summary>
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    ///     توضیح قید (اختیاری)
    /// </summary>
    public string? Description { get; set; }
}