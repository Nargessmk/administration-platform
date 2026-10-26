namespace Cloudland.APM.Microservices.AppManagement.Services;

public class ReindexResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ProcessedCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string>? Details { get; set; }

    public static ReindexResult Success(int processedCount, int errorCount, List<string>? details = null)
    {
        return new ReindexResult
        {
            IsSuccess = true,
            Message = "Reindex completed successfully",
            ProcessedCount = processedCount,
            ErrorCount = errorCount,
            Details = details
        };
    }

    public static ReindexResult Failure(string message)
    {
        return new ReindexResult
        {
            IsSuccess = false,
            Message = message,
            ProcessedCount = 0,
            ErrorCount = 0
        };
    }
}
