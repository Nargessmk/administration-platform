using System.Data;
using Dapper;

namespace Cloudland.APM.Utils.DQL;

public class DatabaseMetadataProvider(IDbConnection connection)
{
    public List<TableMeta> Tables { get; } = [];

    public void LoadMetadata()
    {
        // این متد، اطلاعات جداول، ستون‌ها، description فارسی و نام اصلی را بارگذاری می‌کند
        const string sql = """

                                       SELECT 
                                           t.name AS TableName, 
                                           ep.value AS TableDescription,
                                           c.name AS ColumnName, 
                                           c.column_id,
                                           ep2.value AS ColumnDescription
                                       FROM sys.tables t
                                       LEFT JOIN sys.extended_properties ep ON t.object_id = ep.major_id AND ep.minor_id = 0 AND ep.name = N'MS_Description'
                                       INNER JOIN sys.columns c ON t.object_id = c.object_id
                                       LEFT JOIN sys.extended_properties ep2 ON c.object_id = ep2.major_id AND c.column_id = ep2.minor_id AND ep2.name = N'MS_Description'
                                       ORDER BY t.name, c.column_id
                           """;

        var items = connection.Query<MetaItem>(sql).ToList();
        // ساخت mapping بین فارسی و انگلیسی
        var tableGroups = items.GroupBy(x => x.TableName);
        foreach (var tbl in tableGroups)
        {
            var tableMeta = new TableMeta
            {
                TableName = tbl.Key,
                Description = tbl.FirstOrDefault(x => !string.IsNullOrEmpty(x.TableDescription))?.TableDescription ??
                              tbl.Key,
                Columns = tbl.Select(c => new ColumnMeta
                {
                    ColumnName = c.ColumnName,
                    Description = c.ColumnDescription
                }).ToList()
            };
            Tables.Add(tableMeta);
        }
    }
}