namespace Cloudland.APM.Models.ChatGPT;

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Sender { get; set; } = "user"; // "user" یا "assistant"
    public string? Content { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}