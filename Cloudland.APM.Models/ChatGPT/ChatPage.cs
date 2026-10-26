namespace Cloudland.APM.Models.ChatGPT;

public class ChatPage
{
    public string? Id { get; set; }
    public required string UserId { get; set; }
    public required string Title { get; set; }
    public List<ChatMessage> History { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}