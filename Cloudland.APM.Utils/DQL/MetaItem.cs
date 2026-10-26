namespace Cloudland.APM.Utils.DQL;

public class MetaItem
{
    public string TableName { get; set; } = string.Empty;
    public string TableDescription { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public int ColumnId { get; set; }
    public string ColumnDescription { get; set; } = string.Empty;
}