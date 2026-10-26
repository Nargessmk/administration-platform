// IChatService.cs

using Cloudland.APM.Models.ChatGPT;
using Cloudland.APM.Models.RequestModel;

namespace Cloudland.APM.Microservices.LocalChatGPT.Services;

public interface IChatService
{
    Task<ChatPage?> GetOrCreateChatPageAsync(string userId, string? chatPageId = null);

    Task<ChatMessage> SendMessageAsync(
        string userId,
        string chatPageId,
        string userMessage,
        string? jwtToken,
        string? model = null);

    Task<ChatMessage> SendMessageWithFilesAsync(
        string userId,
        string chatPageId,
        string model,
        string? userInput,
        List<IFormFile>? files,
        string? jwtToken);

    /// <summary>
    ///     ارسال پیام به‌صورت استریم؛ نمایش تکه‌تکه پاسخ
    /// </summary>
    IAsyncEnumerable<string> SendMessageStreamAsync(
        string userId,
        string chatPageId,
        string? model,
        string? jwtToken,
        MessageRequest message,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     ارسال پیام با فایل‌ها به‌صورت استریم؛ نمایش تکه‌تکه پاسخ
    /// </summary>
    IAsyncEnumerable<string> SendMessageWithFilesStreamAsync(
        string userId,
        string chatPageId,
        string? model,
        string? userInput,
        List<IFormFile>? files,
        string? jwtToken,
        CancellationToken cancellationToken = default);

    Task<List<ChatMessage>> GetChatHistoryAsync(string userId, string chatPageId);
    Task<List<ChatPage>> GetUserChatPagesAsync(string userId);
    Task<bool> DeleteMessageAsync(string userId, string chatPageId, string messageId);
    Task<ChatMessage> EditMessageAsync(string userId, string chatPageId, string messageId, string newContent);
    Task<bool> DeleteChatPageAsync(string userId, string chatPageId);
    Task<List<ChatMessage>> SearchMessagesAsync(string userId, string chatPageId, string? searchText);
    Task<UserTokenProfile?> GetUserTokenProfileAsync(string userId);
    Task<UserTokenProfile> RechargeTokensAsync(string userId, int amount);

    Task<IEnumerable<string>> GetAvailableModelsAsync(string? jwtToken = null);
    Task<List<string>> UploadFilesAsync(List<IFormFile> files, string? jwtToken, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> SendMessageStreamWithFileIdsAsync(
        string userId,
        string chatPageId,
        string? userInput,
        IEnumerable<string>? fileIds,
        string? model,
        string? jwtToken,
        CancellationToken cancellationToken = default);
}