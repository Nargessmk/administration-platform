using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using ChatMessage = Cloudland.APM.Models.ChatGPT.ChatMessage;

namespace Cloudland.APM.Microservices.ChatGPT.Services;

public class OpenAiGateway : IOpenAiGateway
{
    private readonly OpenAIClient _api;
    private readonly ConcurrentDictionary<string, string> _assistantCache;
    private readonly HttpClient _client;

    public OpenAiGateway(HttpClient client, IConfiguration config)
    {
        _client = client;
        _api = new OpenAIClient(config["OpenAi:ApiKey"]
                                ?? throw new InvalidOperationException("API key missing"));
        _assistantCache = new ConcurrentDictionary<string, string>();

        var apiKey = config["OpenAi:ApiKey"]
                     ?? throw new InvalidOperationException("API key missing");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        _client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
    }

    public async Task<IEnumerable<string>> ListModelsAsync()
    {
        var models = await _api.ModelsEndpoint.GetModelsAsync();
        return models.Select(x => x.ToString()).ToArray();
    }

    public async Task<string?> ChatAsync(
        IEnumerable<ChatMessage> history,
        string model = "gpt-4o")
    {
        var gptModel = new Model(model);
        var chatHistory = new List<Message>();
        history.ToList().ForEach(x =>
        {
            chatHistory.Add(new Message(x.Sender == "user" ? Role.User : Role.Assistant, x.Content, model));
        });
        var chatRequest = new ChatRequest(chatHistory, gptModel);
        var response = await _api.ChatEndpoint.GetCompletionAsync(chatRequest);
        var choice = response.FirstChoice;
        return choice.Message;
    }

    public async Task<string?> ChatWithWebSearchAsync(
        IEnumerable<ChatMessage> history,
        string model = "gpt-4.1")
    {
        var request = new Dictionary<string, object>
        {
            ["model"] = model,
            ["input"] = history.Select(m => new { role = m.Sender, content = m.Content }),
            ["tools"] = new object[]
            {
                new { type = "web_search_preview" }
            }
        };

        var payload = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync(
            "https://api.openai.com/v1/responses",
            payload
        );
        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
    }


    public async IAsyncEnumerable<string> ChatAsyncStream(
        IEnumerable<ChatMessage> history,
        string model = "gpt-4.1",
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            model,
            messages = history.Select(m => new { role = m.Sender, content = m.Content }).ToList(),
            stream = true
        };

        var json = JsonSerializer.Serialize(requestBody);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var requestMessage = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.openai.com/v1/chat/completions");
        requestMessage.Content = content;

        using var response = await _client.SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var responseStream = await response
            .Content
            .ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(responseStream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            var payload = line["data: ".Length..];
            if (payload == "[DONE]")
                yield break;

            if (!string.IsNullOrEmpty(payload))
                yield return payload;
        }
    }


    public async IAsyncEnumerable<string> ChatWithWebSearchAsyncStream(
        IEnumerable<ChatMessage> history,
        string model = "gpt-4.1",
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new Dictionary<string, object>
        {
            ["model"] = model,
            ["input"] = history.Select(m => new { role = m.Sender, content = m.Content }),
            ["tool_choice"] = "auto",
            ["stream"] = "true",
            ["tools"] = new object[]
            {
                new { type = "web_search_preview" },
                new { type = "code_interpreter", container = new { type = "auto" } }
            }
        };

        var requestPayload = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync(
            "https://api.openai.com/v1/responses",
            requestPayload, cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var responseStream = await response
            .Content
            .ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(responseStream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            var payload = line["data: ".Length..];

            if (!string.IsNullOrEmpty(payload))
                yield return payload;

            if (payload == "[DONE]")
                yield break;
        }
    }

    private async Task<string> GetOrCreateAssistantIdAsync(string modelId)
    {
        if (_assistantCache.TryGetValue(modelId, out var existingId))
            return existingId;

        var available = await ListModelsAsync();
        if (!available.Contains(modelId))
            throw new ArgumentException($"Model '{modelId}' is not available.");

        var payload = new
        {
            name = $"assistant-{modelId}-{Guid.NewGuid()}",
            model = modelId,
            instructions = "You are a helpful assistant that processes user queries and files.",
            tools = new[]
            {
                new { type = "code_interpreter" },
                new { type = "file_search" }
            }
        };

        var response = await _client.PostAsync(
            "https://api.openai.com/v1/assistants",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var assistantId = doc.RootElement.GetProperty("id").GetString();
        _assistantCache[modelId] = assistantId ?? throw new Exception();
        return assistantId;
    }

    public async Task<string?> ChatWithFilesByModelAsync(
        string modelId,
        string userInput,
        IEnumerable<IFormFile> files)
    {
        var assistantId = await GetOrCreateAssistantIdAsync(modelId);

        // آپلود فایل‌ها
        var fileIds = new List<string>();
        foreach (var file in files.Where(f => f.Length > 0))
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);
            form.Add(new StringContent("assistants"), "purpose");

            var fileRes = await _client.PostAsync("https://api.openai.com/v1/files", form);
            fileRes.EnsureSuccessStatusCode();

            using var fdoc = JsonDocument.Parse(await fileRes.Content.ReadAsStringAsync());
            var id = fdoc.RootElement.GetProperty("id").GetString();
            fileIds.Add(id ?? throw new Exception());
        }

        // ساخت thread جدید
        var threadRes = await _client.PostAsync(
            "https://api.openai.com/v1/threads",
            new StringContent("{}", Encoding.UTF8, "application/json"));
        threadRes.EnsureSuccessStatusCode();

        using var tdoc = JsonDocument.Parse(await threadRes.Content.ReadAsStringAsync());
        var threadId = tdoc.RootElement.GetProperty("id").GetString();

        var msgPayload = new
        {
            role = "user",
            content = userInput,
            attachments = fileIds
                .Select(fid => new { file_id = fid, tools = new[] { new { type = "file_search" } } })
        };

        var msgRes = await _client.PostAsync(
            $"https://api.openai.com/v1/threads/{threadId}/messages",
            new StringContent(JsonSerializer.Serialize(msgPayload), Encoding.UTF8, "application/json"));
        msgRes.EnsureSuccessStatusCode();

        // ساخت Run جدید
        var runRes = await _client.PostAsync(
            $"https://api.openai.com/v1/threads/{threadId}/runs",
            new StringContent(JsonSerializer.Serialize(new { assistant_id = assistantId }), Encoding.UTF8,
                "application/json"));
        runRes.EnsureSuccessStatusCode();

        using var rdoc = JsonDocument.Parse(await runRes.Content.ReadAsStringAsync());
        var runId = rdoc.RootElement.GetProperty("id").GetString();

        while (true)
        {
            var statusRes = await _client.GetAsync(
                $"https://api.openai.com/v1/threads/{threadId}/runs/{runId}");
            statusRes.EnsureSuccessStatusCode();

            using var sdoc = JsonDocument.Parse(await statusRes.Content.ReadAsStringAsync());
            var status = sdoc.RootElement.GetProperty("status").GetString();
            if (status is "completed" or "failed" or "cancelled")
                break;

            await Task.Delay(1000);
        }

        // خواندن پیام‌های نهایی
        var listRes = await _client.GetAsync(
            $"https://api.openai.com/v1/threads/{threadId}/messages");
        listRes.EnsureSuccessStatusCode();

        using var ldoc = JsonDocument.Parse(await listRes.Content.ReadAsStringAsync());
        return ldoc.RootElement
            .GetProperty("data")[0]
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetProperty("value")
            .GetString();
    }

    public async IAsyncEnumerable<string> ChatWithFilesByModelStreamAsync(
        string modelId,
        string userInput,
        IEnumerable<IFormFile> files,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var assistantId = await GetOrCreateAssistantIdAsync(modelId);

        var fileIds = new List<string>();
        foreach (var file in files.Where(f => f.Length > 0))
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);
            form.Add(new StringContent("assistants"), "purpose");

            var fileRes = await _client.PostAsync("https://api.openai.com/v1/files", form, cancellationToken);
            fileRes.EnsureSuccessStatusCode();

            using var fdoc = JsonDocument.Parse(
                await fileRes.Content.ReadAsStringAsync(cancellationToken));
            var id = fdoc.RootElement.GetProperty("id").GetString();
            if (id != null)
                fileIds.Add(id);
        }

        var attachments = fileIds
            .Select(fid => new
            {
                file_id = fid,
                tools = new[] { new { type = "file_search" } }
            })
            .ToArray();

        var requestBody = new
        {
            model = $"assistant:{assistantId}",
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = userInput,
                    attachments
                }
            },
            stream = true
        };

        var json = JsonSerializer.Serialize(requestBody);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var requestMessage = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.openai.com/v1/chat/completions");
        requestMessage.Content = content;

        using var response = await _client.SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var responseStream = await response
            .Content
            .ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(responseStream);

        // ۵. خواندن استریم خط‌به‌خط
        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            var payload = line["data: ".Length..];
            if (payload == "[DONE]")
                yield break;

            string? chunk = null;
            try
            {
                using var doc = JsonDocument.Parse(payload);
                var choices = doc.RootElement.GetProperty("choices");
                var delta = choices[0].GetProperty("delta");
                if (delta.TryGetProperty("content", out var contentProp))
                    chunk = contentProp.GetString();
            }
            catch
            {
                continue;
            }

            if (!string.IsNullOrEmpty(chunk))
                yield return chunk;
        }
    }

    public async Task<IEnumerable<string>> UploadFilesAsync(
        IEnumerable<IFormFile> files,
        CancellationToken cancellationToken = default)
    {
        var fileIds = new List<string>();
        foreach (var file in files.Where(f => f.Length > 0))
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);
            form.Add(new StringContent("assistants"), "purpose");
            var fileResponse = await _client.PostAsync(
                "https://api.openai.com/v1/files",
                form,
                cancellationToken);

            fileResponse.EnsureSuccessStatusCode();
            using var doc = JsonDocument.Parse(
                await fileResponse.Content.ReadAsStringAsync(cancellationToken));

            var fileId = doc.RootElement.GetProperty("id").GetString();

            if (!string.IsNullOrEmpty(fileId))
            {
                fileIds.Add(fileId);
            }
        }

        return fileIds;
    }

    public async IAsyncEnumerable<string> ChatAsyncStream(
        IEnumerable<ChatMessage> history,
        IEnumerable<string>? fileIds,
        string model = "gpt-4.1",
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var historyList = history.ToList();
        if (historyList.Count == 0)
        {
            yield break;
        }
        var fileIdList = fileIds?.ToList();
        var apiMessages = new List<object>();
        apiMessages.AddRange(historyList.Take(historyList.Count - 1)
            .Select(m => new { role = m.Sender, content = m.Content }));

        var lastMessage = historyList.Last();

        if (fileIdList is { Count: > 0 })
        {
            var attachments = fileIdList.Select(id => new
            {
                file_id = id,
                tools = new[] { new { type = "file_search" } }
            }).ToList();

            apiMessages.Add(new
            {
                role = lastMessage.Sender,
                content = lastMessage.Content,
                attachments
            });
        }
        else
        {
            apiMessages.Add(new { role = lastMessage.Sender, content = lastMessage.Content });
        }

        var requestBody = new
        {
            model,
            messages = apiMessages,
            stream = true
        };

        var json = JsonSerializer.Serialize(requestBody);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var requestMessage = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.openai.com/v1/chat/completions");
        requestMessage.Content = content;

        using var response = await _client.SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(responseStream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            var payload = line["data: ".Length..];
            if (payload == "[DONE]")
                yield break;

            if (!string.IsNullOrEmpty(payload))
                yield return payload;
        }
    }
}