namespace Cloudland.APM.Utils.DQL;

public class TableMeta
{
    public string TableName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ColumnMeta>? Columns { get; set; }
}