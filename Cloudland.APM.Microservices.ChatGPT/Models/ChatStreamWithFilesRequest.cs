using Cloudland.APM.Models.ChatGPT;

namespace Cloudland.APM.Microservices.ChatGPT.Models;

public class ChatStreamWithFilesRequest
{
    public required IEnumerable<ChatMessage> History { get; set; }
    public IEnumerable<string>? FileIds { get; set; }
}