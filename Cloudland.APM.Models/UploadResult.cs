namespace Cloudland.APM.Models;

public class UploadResult
{
    public bool IsSuccess { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? ErrorMessage { get; private set; }
    public UploadErrorType ErrorType { get; private set; }

    private UploadResult(bool isSuccess, string? imageUrl = null, string? errorMessage = null, UploadErrorType errorType = UploadErrorType.None)
    {
        IsSuccess = isSuccess;
        ImageUrl = imageUrl;
        ErrorMessage = errorMessage;
        ErrorType = errorType;
    }

    public static UploadResult Success(string imageUrl) => new(true, imageUrl);
    public static UploadResult Failure(string errorMessage, UploadErrorType errorType) => new(false, errorMessage: errorMessage, errorType: errorType);
}

public enum UploadErrorType
{
    None,
    UserNotFound,
    InvalidFile,
    FileTooLarge,
    InvalidFileType,
    FileSystemError,
    FusionAuthError,
    SystemError
}
