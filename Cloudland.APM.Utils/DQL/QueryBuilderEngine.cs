using System.Text;

namespace Cloudland.APM.Utils.DQL;

public class QueryBuilderEngine(List<TableMeta> tables)
{
    public List<TableMeta> tables { get; } = tables;

    public string Build(QueryDto query)
    {
        // 1. SELECT clause
        var selectList = new List<string>();
        if (query.IsDistinct)
            selectList.Add("DISTINCT");
        if (query.SelectedColumns.Count > 0)
            selectList.AddRange(query.SelectedColumns);
        if (query.AggregateColumns.Count > 0)
            selectList.AddRange(query.AggregateColumns);
        if (selectList.Count == 0)
            selectList.Add("*");

        var selectClause = $"SELECT {string.Join(", ", selectList)}";

        string fromClause;
        var fromSubQuery =
            query.SubQueries.FirstOrDefault(sq => sq.Place.Equals("FROM", StringComparison.OrdinalIgnoreCase));
        if (fromSubQuery is { SubQuery: not null })
        {
            var subquerySql = Build(fromSubQuery.SubQuery);
            fromClause = $"FROM ({subquerySql}) AS SubTbl";
        }
        else
        {
            fromClause = $"FROM [{query.Table}]";
        }

        var joinClauses = new StringBuilder();
        foreach (var join in query.Joins)
            joinClauses.AppendLine($@" {join.JoinType.ToUpper()} JOIN [{join.TargetTable}]
                        ON [{query.Table}].[{join.SourceColumn}] = [{join.TargetTable}].[{join.TargetColumn}]");

        var whereConditions = new List<string>();
        if (query.WhereConditions.Count > 0)
            whereConditions.AddRange(query.WhereConditions);

        whereConditions.AddRange(
            from sub in query.SubQueries.Where(sq => sq.Place.Equals("WHERE", StringComparison.OrdinalIgnoreCase))
            let actualSubQuery = sub.SubQuery where actualSubQuery != null
            let subTable = actualSubQuery.Table ?? "SubTbl"
            let subColumn = actualSubQuery.SelectedColumns.FirstOrDefault() ?? "Id"
            let subquerySql = Build(actualSubQuery)
            select $"{subTable}.{subColumn} IN ({subquerySql})");
        var whereClause = whereConditions.Count > 0 ? "WHERE " + string.Join(" AND ", whereConditions) : "";

        var groupByClause = query.GroupByColumns.Count > 0
            ? "GROUP BY " + string.Join(", ", query.GroupByColumns)
            : "";

        var havingClause = query.HavingConditions.Count > 0
            ? "HAVING " + string.Join(" AND ", query.HavingConditions)
            : "";

        var orderByClause = query.OrderByColumns.Count > 0
            ? "ORDER BY " + string.Join(", ", query.OrderByColumns)
            : "";

        var limitClause = "";
        if (query.Limit.HasValue)
            limitClause = $"OFFSET 0 ROWS FETCH NEXT {query.Limit.Value} ROWS ONLY";

        var sql = $"""
                   {selectClause}
                   {fromClause}
                   {joinClauses}
                   {whereClause}
                   {groupByClause}
                   {havingClause}
                   {orderByClause}
                   {limitClause}
                   """.Trim();

        if (query.UnionQueries is not { Count: > 0 }) return sql;
        var unions = new List<string> { sql };
        unions.AddRange(query.UnionQueries.Select(Build));
        sql = string.Join("\nUNION ALL\n", unions);

        return sql;
    }
}