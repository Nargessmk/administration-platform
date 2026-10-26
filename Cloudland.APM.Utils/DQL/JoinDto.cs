namespace Cloudland.APM.Utils.DQL;

public class JoinDto
{
    public string JoinType { get; set; } = string.Empty; // INNER, LEFT, RIGHT, FULL
    public string TargetTable { get; set; } = string.Empty;
    public string TargetTableDescription { get; set; } = string.Empty;
    public string SourceColumn { get; set; } = string.Empty;
    public string TargetColumn { get; set; } = string.Empty;
}