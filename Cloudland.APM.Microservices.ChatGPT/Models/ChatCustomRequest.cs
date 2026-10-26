namespace Cloudland.APM.Microservices.ChatGPT.Models;

public class ChatCustomRequest
{
    public string Model { get; set; } = "gpt-4.1";
    public string UserInput { get; set; } = string.Empty;
    public required List<IFormFile> Files { get; set; }
}