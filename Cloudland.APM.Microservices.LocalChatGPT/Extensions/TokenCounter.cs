using TiktokenSharp;

namespace Cloudland.APM.Microservices.LocalChatGPT.Extensions;

public class TokenCounter
{
    private readonly TikToken _tokenizer = TikToken.EncodingForModel("gpt-3.5-turbo");

    public int CountTokens(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;
        return string.IsNullOrWhiteSpace(text) ? 0 : _tokenizer.Encode(text).Count;
    }
}