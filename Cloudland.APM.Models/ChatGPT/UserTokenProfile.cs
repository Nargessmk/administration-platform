namespace Cloudland.APM.Models.ChatGPT;

public class UserTokenProfile
{
    public required string Id { get; set; } // "UserTokens/{userId}"
    public required string UserId { get; set; }
    public int TokenBalance { get; set; }
    public int InputTokensUsed { get; set; }
    public int OutputTokensUsed { get; set; }
}