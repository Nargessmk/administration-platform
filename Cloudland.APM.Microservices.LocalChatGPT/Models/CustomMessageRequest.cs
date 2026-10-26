namespace Cloudland.APM.Microservices.LocalChatGPT.Models;

public class CustomMessageRequest
{
    public string Model { get; set; } = "gpt-4.1";
    public string? UserInput { get; set; }
    public List<IFormFile>? Files { get; set; }
}