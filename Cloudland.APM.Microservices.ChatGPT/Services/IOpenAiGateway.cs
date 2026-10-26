using Cloudland.APM.Models.ChatGPT;

namespace Cloudland.APM.Microservices.ChatGPT.Services;

public interface IOpenAiGateway
{
    Task<IEnumerable<string>> ListModelsAsync();

    Task<string?> ChatAsync(
        IEnumerable<ChatMessage> history,
        string model = "gpt-4o");

    Task<string?> ChatWithWebSearchAsync(
        IEnumerable<ChatMessage> history,
        string model = "gpt-4.1");

    IAsyncEnumerable<string> ChatAsyncStream(
        IEnumerable<ChatMessage> history,
        string model = "gpt-4.1",
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> ChatWithWebSearchAsyncStream(
        IEnumerable<ChatMessage> history,
        string model = "gpt-4.1",
        CancellationToken cancellationToken = default);

    Task<string?> ChatWithFilesByModelAsync(
        string modelId,
        string userInput,
        IEnumerable<IFormFile> files);

    IAsyncEnumerable<string> ChatWithFilesByModelStreamAsync(
        string modelId,
        string userInput,
        IEnumerable<IFormFile> files,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<string>> UploadFilesAsync(
        IEnumerable<IFormFile> files,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> ChatAsyncStream(
        IEnumerable<ChatMessage> history,
        IEnumerable<string>? fileIds,
        string model = "gpt-4.1",
        CancellationToken cancellationToken = default);
}