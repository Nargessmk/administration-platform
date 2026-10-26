namespace Cloudland.APM.Models.ResultModel;

public class PagedResult<T>
{
    public required List<T> Items { get; set; } // The actual data 
    public int TotalCount { get; set; } // Total records in DB (for all pages)
    public int PageIndex { get; set; } // Current page (1-based)
    public int PageSize { get; set; } // Items per page
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize); // Calculated
}