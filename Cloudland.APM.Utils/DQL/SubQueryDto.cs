namespace Cloudland.APM.Utils.DQL;

public class SubQueryDto
{
    public string Place { get; set; } = string.Empty; // WHERE, SELECT, FROM
    public QueryDto? SubQuery { get; set; }
}