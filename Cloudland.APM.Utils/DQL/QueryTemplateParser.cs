using System.Text.RegularExpressions;

namespace Cloudland.APM.Utils.DQL;

public class QueryTemplateParser(List<TableMeta> tables)
{
    public QueryDto Parse(string farsiQuery)
    {
        var query = new QueryDto();

        /*// --- شناسایی جدول اصلی ---
        foreach (var tbl in tables.Where(tbl => farsiQuery.Contains(tbl.Description)))
        {
            query.Table = tbl.TableName;
            query.TableDescription = tbl.Description;
            break;
        }*/

        var bestMatch = tables
            .Select(tbl => new
            {
                Table = tbl,
                MatchScore = CountMatchingWords(farsiQuery, tbl.Description),
                DescriptionLength = tbl.Description.Length
            })
            .Where(x => x.MatchScore > 0) // حداقل یک کلمه مشترک داشته باشد
            .OrderByDescending(x => x.MatchScore) // بیشترین تطابق کلمات
            .ThenBy(x => x.DescriptionLength) // در صورت تساوی، توضیح کوتاه‌تر انتخاب شود
            .FirstOrDefault();

        if (bestMatch != null)
        {
            query.Table = bestMatch.Table.TableName;
            query.TableDescription = bestMatch.Table.Description;
        }

        static int CountMatchingWords(string text, string phrase)
        {
            var textWords = text.Split([' ', '\t', '\n'], StringSplitOptions.RemoveEmptyEntries);
            var phraseWords = phrase.Split([' ', '\t', '\n'], StringSplitOptions.RemoveEmptyEntries);

            return phraseWords.Count(word => textWords.Contains(word));
        }

        if (string.IsNullOrEmpty(query.Table))
            return query;

        var columns = tables.First(t => t.TableName == query.Table).Columns;
        if (columns == null) throw new Exception($"Table {query.Table} not found");
        foreach (var col in columns.Where(col => farsiQuery.Contains(col.Description)))
            query.SelectedColumns.Add(col.ColumnName);

        AddBasicWhereConditions(farsiQuery, columns, query);

        var matchGroupBy = Regex.Match(farsiQuery, @"به تفکیک (\S+)");
        if (matchGroupBy.Success)
        {
            var gbCol = columns.FirstOrDefault(c => c.Description == matchGroupBy.Groups[1].Value);
            if (gbCol != null)
                query.GroupByColumns.Add(gbCol.ColumnName);
        }

        var groupByMatches = Regex.Matches(farsiQuery, @"به تفکیک (\S+)");
        foreach (Match m in groupByMatches)
        {
            var gbCol = columns.FirstOrDefault(c => c.Description == m.Groups[1].Value);
            if (gbCol != null && !query.GroupByColumns.Contains(gbCol.ColumnName))
                query.GroupByColumns.Add(gbCol.ColumnName);
        }

        var matchHaving = Regex.Match(farsiQuery, @"گروه‌هایی که جمع (\S+) (بزرگتر|کوچکتر|برابر|مساوی) از (\d+)");
        if (matchHaving.Success)
        {
            var gbCol = columns.FirstOrDefault(c => c.Description == matchHaving.Groups[1].Value);
            if (gbCol != null)
            {
                var op = matchHaving.Groups[2].Value switch
                {
                    "بزرگتر" => ">",
                    "کوچکتر" => "<",
                    "برابر" or "مساوی" => "=",
                    _ => throw new ArgumentOutOfRangeException()
                };
                query.HavingConditions.Add($"SUM({gbCol.ColumnName}) {op} {matchHaving.Groups[3].Value}");
            }
        }

        var matchOrderBy = Regex.Match(farsiQuery, @"مرتب سازی بر اساس (\S+)");
        if (matchOrderBy.Success)
        {
            var obCol = columns.FirstOrDefault(c => c.Description == matchOrderBy.Groups[1].Value);
            if (obCol != null)
                query.OrderByColumns.Add(obCol.ColumnName);
        }

        foreach (var t in tables.Where(t => t.TableName != query.Table).Where(t => farsiQuery.Contains(t.Description)))
        {
            if (t.Columns == null) throw new Exception();
            var targetCol = t.Columns.FirstOrDefault(c => c.ColumnName.ToLower().Contains("id"));
            var sourceCol = columns.FirstOrDefault(c => c.ColumnName.ToLower().Contains("id"));
            if (targetCol != null && sourceCol != null)
                query.Joins.Add(new JoinDto
                {
                    JoinType = "INNER", 
                    TargetTable = t.TableName,
                    TargetTableDescription = t.Description,
                    SourceColumn = sourceCol.ColumnName,
                    TargetColumn = targetCol.ColumnName
                });
        }

        if (farsiQuery.Contains("همه") && farsiQuery.Contains("حتی اگر"))
            foreach (var join in query.Joins)
                join.JoinType = "LEFT";
        
        var matchSubQuery = Regex.Match(farsiQuery, @"در خروجی پرس‌وجوی \[(.+?)\] باشد");
        if (matchSubQuery.Success)
        {
            var subQueryText = matchSubQuery.Groups[1].Value;
            var subParser = new QueryTemplateParser(tables);
            var subQueryDto = subParser.Parse(subQueryText);

            query.SubQueries.Add(new SubQueryDto
            {
                Place = "WHERE",
                SubQuery = subQueryDto
            });
        }

        var matchLimit = Regex.Match(farsiQuery, @"(\d+)\s*سطر اول");
        if (matchLimit.Success) query.Limit = int.Parse(matchLimit.Groups[1].Value);

        if (farsiQuery.Contains("بدون تکرار") || farsiQuery.Contains("distinct"))
            query.IsDistinct = true;

        return query;
    }

    private void AddBasicWhereConditions(string farsiQuery, List<ColumnMeta> columns, QueryDto query)
    {
        // >, <, =
        var matchCompare = Regex.Match(farsiQuery, @"(\S+)\s*(بزرگتر|کوچکتر|برابر|مساوی)\s*از\s*(\d+)");
        if (matchCompare.Success)
        {
            var col = columns.FirstOrDefault(c => c.Description == matchCompare.Groups[1].Value);
            if (col != null)
            {
                var op = matchCompare.Groups[2].Value switch
                {
                    "بزرگتر" => ">",
                    "کوچکتر" => "<",
                    "برابر" or "مساوی" => "=",
                    _ => throw new ArgumentOutOfRangeException()
                };
                query.WhereConditions.Add($"{col.ColumnName} {op} {matchCompare.Groups[3].Value}");
            }
        }

        // LIKE
        var matchLike = Regex.Match(farsiQuery, @"(\S+)\s*(شامل|حاوی|مانند|مثل)\s*(\S+)");
        if (matchLike.Success)
        {
            var col = columns.FirstOrDefault(c => c.Description == matchLike.Groups[1].Value);
            if (col != null)
            {
                var pattern = matchLike.Groups[3].Value;
                query.WhereConditions.Add($"{col.ColumnName} LIKE N'%{pattern}%'");
            }
        }

        // IN
        var matchIn = Regex.Match(farsiQuery, @"(\S+)\s*در\s*\[([^\]]+)\]\s*باشد");
        if (matchIn.Success)
        {
            var col = columns.FirstOrDefault(c => c.Description == matchIn.Groups[1].Value);
            if (col != null)
            {
                var values = matchIn.Groups[2].Value.Split('،', ' ').Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(v => $"N'{v.Trim()}'");
                query.WhereConditions.Add($"{col.ColumnName} IN ({string.Join(", ", values)})");
            }
        }

        // BETWEEN
        var matchBetween = Regex.Match(farsiQuery, @"(\S+)\s*بین\s*(\d+)\s*و\s*(\d+)\s*باشد");
        if (matchBetween.Success)
        {
            var col = columns.FirstOrDefault(c => c.Description == matchBetween.Groups[1].Value);
            if (col != null)
                query.WhereConditions.Add(
                    $"{col.ColumnName} BETWEEN {matchBetween.Groups[2].Value} AND {matchBetween.Groups[3].Value}");
        }

        // IS NULL
        var matchNull = Regex.Match(farsiQuery, @"(\S+)\s*خالی باشد");
        if (matchNull.Success)
        {
            var col = columns.FirstOrDefault(c => c.Description == matchNull.Groups[1].Value);
            if (col != null) query.WhereConditions.Add($"{col.ColumnName} IS NULL");
        }

        // IS NOT NULL
        var matchNotNull = Regex.Match(farsiQuery, @"(\S+)\s*مقدار داشته باشد");
        if (!matchNotNull.Success) return;
        {
            var col = columns.FirstOrDefault(c => c.Description == matchNotNull.Groups[1].Value);
            if (col != null) query.WhereConditions.Add($"{col.ColumnName} IS NOT NULL");
        }
    }
}