namespace Cloudland.APM.Models;

public class FileUploadSettings
{
    public required string UploadPath { get; set; }
    public required string[] AllowedExtensions { get; set; }
    public int MaxFileSizeMB { get; set; }
    public required string BaseUrl { get; set; }
}