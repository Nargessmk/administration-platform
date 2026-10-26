namespace Cloudland.APM.Microservices.LocalChatGPT.Models;

public class MessageWithFileIdsRequest
{
    public string? UserInput { get; set; }
    public IEnumerable<string>? FileIds { get; set; }
    public string? ModelName { get; set; }
}