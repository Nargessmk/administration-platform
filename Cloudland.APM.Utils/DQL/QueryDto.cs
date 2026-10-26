namespace Cloudland.APM.Utils.DQL;

public class QueryDto
{
    public string Table { get; set; } = string.Empty;
    public string TableDescription { get; set; } = string.Empty;
    public List<string> SelectedColumns { get; set; } = [];
    public List<string> AggregateColumns { get; set; } = []; // sum(age) as SumAge
    public List<string> WhereConditions { get; set; } = [];
    public List<JoinDto> Joins { get; set; } = [];
    public List<string> GroupByColumns { get; set; } = [];
    public List<string> HavingConditions { get; set; } = [];
    public List<string> OrderByColumns { get; set; } = [];
    public List<SubQueryDto> SubQueries { get; set; } = [];
    public bool IsDistinct { get; set; } = false;
    public int? Limit { get; set; }
    public int? Offset { get; set; }
    public List<QueryDto>? UnionQueries { get; set; } // ✅ باید اینگونه باشد
}