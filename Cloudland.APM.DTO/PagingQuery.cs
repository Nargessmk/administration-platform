namespace Cloudland.APM.DTO;

public class PagingQuery
{
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 50;
    public List<Guid>? Ids { get; set; }
    public string? QueryString { get; set; } = "*";
}