namespace Cloudland.SSSB.DTO;

public class ResultDTO<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new List<string>();
    
    // Optional: Properties for pagination
    public int? TotalCount { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public int? TotalPages { get; set; }

    // Success factory methods
    public static ResultDTO<T> Success(T data, string message = "عملیات موفقیت آمیز")
    {
        return new ResultDTO<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message
        };
    }
    
    public static ResultDTO<T> Success(T data, int totalCount, int pageNumber, int pageSize, string message = "Operation successful")
    {
        return new ResultDTO<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    // Failure factory methods
    public static ResultDTO<T> Failure(string message, List<string>? errors = null)
    {
        return new ResultDTO<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = errors ?? []
        };
    }
    
    public static ResultDTO<T> Failure(string message, T? existingData = default)
    {
        return new ResultDTO<T>
        {
            IsSuccess = false,
            Message = message,
            Data = existingData // Useful for returning the duplicate entity
        };
    }
}