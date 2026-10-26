namespace Cloudland.APM.Models.RequestModel;

public class MessageRequest
{
    public string message { get; set; } = string.Empty;
    public string? modelName { get; set; } = "gpt-4.1";
}