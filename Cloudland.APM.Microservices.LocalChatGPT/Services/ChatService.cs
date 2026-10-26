// ChatService.cs

using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Cloudland.APM.Microservices.LocalChatGPT.Extensions;
using Cloudland.APM.Microservices.LocalChatGPT.Models;
using Cloudland.APM.Models.ChatGPT;
using Cloudland.APM.Models.RequestModel;
using Newtonsoft.Json;
using Raven.Client.Documents;

namespace Cloudland.APM.Microservices.LocalChatGPT.Services;

public class ChatService(RavenDbContext dbContext) : IChatService
{
    // ایجاد یا بازیابی صفحه چت
    public async Task<ChatPage?> GetOrCreateChatPageAsync(string userId, string? chatPageId = null)
    {
        using var session = dbContext.OpenSession();

        if (!string.IsNullOrWhiteSpace(chatPageId))
        {
            var existing = await session.LoadAsync<ChatPage>(chatPageId);
            if (existing is not null) return existing;
        }

        var chatPage = new ChatPage
        {
            UserId = userId,
            Title = "New Chat",
            CreatedAt = DateTime.UtcNow
        };
        await session.StoreAsync(chatPage);
        await session.SaveChangesAsync();
        return chatPage;
    }

    public async Task<ChatMessage> SendMessageAsync(
        string userId,
        string chatPageId,
        string userMessage,
        string? jwtToken,
        string? model = null)
    {
        using var session = dbContext.OpenSession();
        var tokenCounter = new TokenCounter();

        // شمارش توکن‌های ورودی
        var inputTokens = tokenCounter.CountTokens(userMessage);
        var userProfile = await session.LoadAsync<UserTokenProfile>($"UserTokens/{userId}");

        if (userProfile == null)
        {
            userProfile = new UserTokenProfile
            {
                Id = $"UserTokens/{userId}",
                UserId = userId,
                TokenBalance = 0,
                InputTokensUsed = 0,
                OutputTokensUsed = 0
            };
            await session.StoreAsync(userProfile);
        }

        if (userProfile.TokenBalance < inputTokens)
            return new ChatMessage
                { Sender = "assistant", Content = "موجودی توکن کافی نیست. لطفاً حساب خود را شارژ کنید." };

        userProfile.InputTokensUsed += inputTokens;
        userProfile.TokenBalance -= inputTokens;

        // اعتبارسنجی صفحه چت
        var chatPage = await session.LoadAsync<ChatPage>(chatPageId);
        if (chatPage == null || chatPage.UserId != userId)
            return new ChatMessage { Sender = "assistant", Content = "صفحه چت یافت نشد یا دسترسی غیرمجاز است." };

        // افزودن پیام کاربر به تاریخچه
        var userMsg = new ChatMessage { Sender = "user", Content = userMessage };
        chatPage.History.Add(userMsg);

        string responseContent;
        try
        {
            using var httpClient = new HttpClient();
            const string baseUrl = "https://api-remote-chatgpt-v1.ailand.ai/api/OpenAi/chat";
            var requestUrl = string.IsNullOrWhiteSpace(model)
                ? baseUrl
                : $"{baseUrl}/{model}";

            if (!string.IsNullOrEmpty(jwtToken))
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", jwtToken.Replace("Bearer ", string.Empty));

            var httpResponse = await httpClient.PostAsJsonAsync(requestUrl, chatPage.History);
            httpResponse.EnsureSuccessStatusCode();
            responseContent =
                JsonConvert
                    .DeserializeObject<ChatMessageContentResult>(await httpResponse.Content.ReadAsStringAsync())?
                    .result
                ?? string.Empty;
        }
        catch (Exception)
        {
            // بازگردانی در صورت خطا
            chatPage.History.Remove(userMsg);
            await session.SaveChangesAsync();
            return new ChatMessage { Sender = "assistant", Content = "سرویس OpenAI پاسخگو نیست." };
        }

        // شمارش توکن‌های خروجی و به‌روزرسانی موجودی
        var outputTokens = tokenCounter.CountTokens(responseContent);
        if (userProfile.TokenBalance < outputTokens)
            return new ChatMessage { Sender = "assistant", Content = "موجودی توکن برای دریافت پاسخ کافی نیست." };

        userProfile.OutputTokensUsed += outputTokens;
        userProfile.TokenBalance -= outputTokens;

        // افزودن پیام دستیار به تاریخچه
        var assistantMsg = new ChatMessage { Sender = "assistant", Content = responseContent };
        chatPage.History.Add(assistantMsg);

        await session.SaveChangesAsync();
        return assistantMsg;
    }

    public async IAsyncEnumerable<string> SendMessageStreamAsync(
        string userId,
        string chatPageId,
        string? model,
        string? jwtToken,
        MessageRequest message,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var session = dbContext.OpenSession();
        var tokenCounter = new TokenCounter();

        var chatPage = await session.LoadAsync<ChatPage>(chatPageId, cancellationToken)
                       ?? throw new Exception("صفحه چت یافت نشد یا دسترسی غیرمجاز است.");

        if (chatPage.UserId != userId)
            throw new Exception("دسترسی غیرمجاز.");

        var historyText = $"{string.Join("\n", chatPage.History.Select(m => m.Content))}";
        var inputTokens = tokenCounter.CountTokens(historyText) + tokenCounter.CountTokens(message.message);

        var userProfile = await session.LoadAsync<UserTokenProfile>($"UserTokens/{userId}", cancellationToken)
                          ?? new UserTokenProfile
                          {
                              Id = $"UserTokens/{userId}",
                              UserId = userId,
                              TokenBalance = 0,
                              InputTokensUsed = 0,
                              OutputTokensUsed = 0
                          };

        if (userProfile.TokenBalance < inputTokens)
            throw new Exception("موجودی توکن کافی برای ارسال پیام نیست.");

        userProfile.InputTokensUsed += inputTokens;
        userProfile.TokenBalance -= inputTokens;
        await session.SaveChangesAsync(cancellationToken);

        const string baseUrl = "https://api-remote-chatgpt-v1.ailand.ai/api/OpenAi/chat";
        // const string baseUrl = "http://localhost:50002/api/OpenAi/chat";
        var requestUrl = string.IsNullOrWhiteSpace(model)
            ? $"{baseUrl}/stream"
            : $"{baseUrl}/stream/{model}";

        using var httpClient = new HttpClient();
        httpClient.Timeout = Timeout.InfiniteTimeSpan;
        if (!string.IsNullOrEmpty(jwtToken))
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwtToken.Replace("Bearer ", string.Empty));

        chatPage.History.Add(new ChatMessage
        {
            Content = message.message,
            Id = Guid.NewGuid().ToString(),
            Sender = "user",
            Timestamp = DateTime.Now
        });
        using var response = await httpClient.PostAsJsonAsync(requestUrl, chatPage.History, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(responseStream);

        var responseBuffer = new StringBuilder();

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;

            var rawChunk = line["data: ".Length..];
            if (rawChunk == "[DONE]") break;

            var chunk = ParseStreamChunk($"{rawChunk}");
            if (string.IsNullOrEmpty(chunk)) continue;

            responseBuffer.Append(chunk);
            yield return chunk;
        }

        var fullResponseText = responseBuffer.ToString();
        var outputTokens = tokenCounter.CountTokens(fullResponseText);

        if (userProfile.TokenBalance < outputTokens)
        {
            chatPage.History.Add(new ChatMessage
            {
                Sender = "assistant",
                Content = "[ناتوان در ثبت پاسخ کامل به دلیل اتمام توکن‌ها]"
            });
        }
        else
        {
            userProfile.OutputTokensUsed += outputTokens;
            userProfile.TokenBalance -= outputTokens;

            chatPage.History.Add(new ChatMessage
            {
                Sender = "assistant",
                Content = fullResponseText
            });
        }

        await session.SaveChangesAsync(cancellationToken);
    }

    // ارسال پیام با فایل‌ها (غیر استریم)
    public async Task<ChatMessage> SendMessageWithFilesAsync(
        string userId,
        string chatPageId,
        string model,
        string? userInput,
        List<IFormFile>? files,
        string? jwtToken)
    {
        using var session = dbContext.OpenSession();
        var tokenCounter = new TokenCounter();

        // شمارش توکن‌های ورودی
        var inputTokens = tokenCounter.CountTokens(userInput ?? string.Empty);
        var userProfile = await session.LoadAsync<UserTokenProfile>($"UserTokens/{userId}")
                          ?? new UserTokenProfile
                          {
                              Id = $"UserTokens/{userId}",
                              UserId = userId,
                              TokenBalance = 0,
                              InputTokensUsed = 0,
                              OutputTokensUsed = 0
                          };

        if (userProfile.InputTokensUsed == 0 && userProfile is { OutputTokensUsed: 0, TokenBalance: 0 })
            await session.StoreAsync(userProfile);

        if (userProfile.TokenBalance < inputTokens)
            throw new Exception("موجودی توکن کافی نیست. لطفاً حساب خود را شارژ کنید.");

        userProfile.InputTokensUsed += inputTokens;
        userProfile.TokenBalance -= inputTokens;

        // اعتبارسنجی صفحه چت
        var chatPage = await session.LoadAsync<ChatPage>(chatPageId)
                       ?? throw new Exception("صفحه چت یافت نشد یا دسترسی غیرمجاز است.");
        if (chatPage.UserId != userId)
            throw new Exception("دسترسی غیرمجاز.");

        // افزودن پیام کاربر به تاریخچه
        var userMsg = new ChatMessage
        {
            Sender = "user",
            Content = userInput
        };
        chatPage.History.Add(userMsg);

        // ساخت prompt با اسامی فایل‌ها
        var attachmentNames = (files ?? Enumerable.Empty<IFormFile>())
            .Select(f => f.FileName)
            .ToList();
        var fileListPrompt = attachmentNames.Any()
            ? $"لطفاً محتوای فایل‌های ضمیمه زیر را بررسی کن: {string.Join(", ", attachmentNames)}.\n\n"
            : string.Empty;
        var finalPrompt = fileListPrompt + (userInput ?? string.Empty);

        string responseContent;
        try
        {
            using var httpClient = new HttpClient();
            const string customUrl = "https://api-remote-chatgpt-v1.ailand.ai/api/OpenAi/chat/custom";
            if (!string.IsNullOrEmpty(jwtToken))
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", jwtToken.Replace("Bearer ", string.Empty));

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(model), "Model");
            form.Add(new StringContent(finalPrompt), "UserInput");

            foreach (var file in files ?? Enumerable.Empty<IFormFile>())
            {
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "Files",
                    FileName = file.FileName
                };
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                form.Add(streamContent);
            }

            var httpResponse = await httpClient.PostAsync(customUrl, form);
            httpResponse.EnsureSuccessStatusCode();
            var json = await httpResponse.Content.ReadAsStringAsync();
            responseContent = JsonConvert
                                  .DeserializeObject<ChatMessageContentResult>(json)
                                  ?.result
                              ?? string.Empty;
        }
        catch (Exception ex)
        {
            chatPage.History.Remove(userMsg);
            await session.SaveChangesAsync();
            throw new Exception("سرویس OpenAI پاسخگو نیست.", ex);
        }

        // شمارش توکن‌های خروجی و به‌روزرسانی موجودی
        var outputTokens = tokenCounter.CountTokens(responseContent);
        if (userProfile.TokenBalance < outputTokens)
            throw new Exception("موجودی توکن برای دریافت پاسخ کافی نیست.");

        userProfile.OutputTokensUsed += outputTokens;
        userProfile.TokenBalance -= outputTokens;

        // افزودن پیام دستیار به تاریخچه
        var assistantMsg = new ChatMessage
        {
            Sender = "assistant",
            Content = responseContent
        };
        chatPage.History.Add(assistantMsg);

        await session.SaveChangesAsync();
        return assistantMsg;
    }

    // ارسال پیام با فایل‌ها به‌صورت استریم؛ نمایش تکه‌تکه پاسخ
    public async IAsyncEnumerable<string> SendMessageWithFilesStreamAsync(
        string userId,
        string chatPageId,
        string? model,
        string? userInput,
        List<IFormFile>? files,
        string? jwtToken,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var session = dbContext.OpenSession();
        var tokenCounter = new TokenCounter();

        // 1️⃣ اعتبارسنجی صفحه چت
        var chatPage = await session.LoadAsync<ChatPage>(chatPageId, cancellationToken)
                       ?? throw new Exception("صفحه چت یافت نشد یا دسترسی غیرمجاز است.");
        if (chatPage.UserId != userId)
            throw new Exception("دسترسی غیرمجاز.");

        // 2️⃣ آماده‌سازی پرامپت نهایی
        var attachmentNames = (files ?? Enumerable.Empty<IFormFile>())
            .Select(f => f.FileName)
            .ToList();
        var fileListPrompt = attachmentNames.Any()
            ? $"لطفاً محتوای فایل‌های ضمیمه زیر را بررسی کن: {string.Join(", ", attachmentNames)}.\n\n"
            : string.Empty;
        var finalPrompt = fileListPrompt + (userInput ?? string.Empty);

        // 3️⃣ شمارش توکن‌های ورودی
        var inputTokens = tokenCounter.CountTokens(finalPrompt);

        // 4️⃣ بارگذاری یا ایجاد پروفایل کاربر
        var userProfile = await session.LoadAsync<UserTokenProfile>($"UserTokens/{userId}", cancellationToken)
                          ?? new UserTokenProfile
                          {
                              Id = $"UserTokens/{userId}",
                              UserId = userId,
                              TokenBalance = 0,
                              InputTokensUsed = 0,
                              OutputTokensUsed = 0
                          };

        if (userProfile.TokenBalance < inputTokens)
            throw new Exception("موجودی توکن کافی برای ارسال پیام نیست.");

        // 5️⃣ کم کردن توکن‌های ورودی
        userProfile.InputTokensUsed += inputTokens;
        userProfile.TokenBalance -= inputTokens;
        await session.SaveChangesAsync(cancellationToken);

        // 6️⃣ افزودن پیام کاربر به تاریخچه
        chatPage.History.Add(new ChatMessage
        {
            Sender = "user",
            Content = finalPrompt,
            Id = Guid.NewGuid().ToString(),
            Timestamp = DateTime.Now
        });
        await session.SaveChangesAsync(cancellationToken);

        // 7️⃣ ارسال درخواست HTTP با فایل‌ها
        using var httpClient = new HttpClient();
        httpClient.Timeout = Timeout.InfiniteTimeSpan;
        if (!string.IsNullOrEmpty(jwtToken))
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwtToken.Replace("Bearer ", string.Empty));

        const string streamUrl = "https://api-remote-chatgpt-v1.ailand.ai/api/OpenAi/chat/custom/stream";

        using var formStream = new MultipartFormDataContent();
        formStream.Add(new StringContent(model ?? ""), "Model");
        formStream.Add(new StringContent(finalPrompt), "UserInput");

        foreach (var file in files ?? Enumerable.Empty<IFormFile>())
        {
            var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "Files",
                FileName = file.FileName
            };
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            formStream.Add(streamContent);
        }

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, streamUrl);
        requestMessage.Content = formStream;

        using var response = await httpClient.SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(responseStream);

        var responseBuffer = new StringBuilder();

        // 8️⃣ پردازش استریم
        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;

            var payload = line["data: ".Length..];
            if (payload == "[DONE]") break;

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

            if (string.IsNullOrEmpty(chunk)) continue;

            responseBuffer.Append(chunk);
            yield return chunk;
        }

        // 9️⃣ ثبت پاسخ کامل و کم کردن توکن خروجی
        var fullResponseText = responseBuffer.ToString();
        var outputTokens = tokenCounter.CountTokens(fullResponseText);

        if (userProfile.TokenBalance < outputTokens)
        {
            chatPage.History.Add(new ChatMessage
            {
                Sender = "assistant",
                Content = "[ناتوان در ثبت پاسخ کامل به دلیل اتمام توکن‌ها]",
                Timestamp = DateTime.Now
            });
        }
        else
        {
            userProfile.OutputTokensUsed += outputTokens;
            userProfile.TokenBalance -= outputTokens;

            chatPage.History.Add(new ChatMessage
            {
                Sender = "assistant",
                Content = fullResponseText,
                Timestamp = DateTime.Now
            });
        }

        await session.SaveChangesAsync(cancellationToken);
    }


    // گرفتن تاریخچه چت
    public async Task<List<ChatMessage>> GetChatHistoryAsync(string userId, string chatPageId)
    {
        using var session = dbContext.OpenSession();
        var chatPage = await session.LoadAsync<ChatPage>(chatPageId);
        if (chatPage == null || !chatPage.UserId.Equals(userId, StringComparison.CurrentCultureIgnoreCase))
            throw new Exception("Not found or denied");
        return chatPage.History;
    }

    // لیست صفحات چت کاربر
    public async Task<List<ChatPage>> GetUserChatPagesAsync(string userId)
    {
        using var session = dbContext.OpenSession();
        var pages = await session.Query<ChatPage>()
            .Where(p => p.History.Any() && p.UserId.Equals(userId, StringComparison.CurrentCultureIgnoreCase))
            .ToListAsync();
        foreach (var page in pages) page.Title = page.History.FirstOrDefault()?.Content ?? "بدون عنوان";

        return pages;
    }

    // حذف پیام
    public async Task<bool> DeleteMessageAsync(string userId, string chatPageId, string messageId)
    {
        using var session = dbContext.OpenSession();
        var chatPage = await session.LoadAsync<ChatPage>(chatPageId);
        if (chatPage == null || chatPage.UserId != userId)
            throw new Exception("Access denied");

        var message = chatPage.History.FirstOrDefault(m => m.Id == messageId);
        if (message == null)
            return false;

        if (message.Sender != "user" || messageId == null)
            throw new Exception("Cannot delete this message");

        chatPage.History.Remove(message);
        await session.SaveChangesAsync();
        return true;
    }

    // ویرایش پیام
    public async Task<ChatMessage> EditMessageAsync(
        string userId,
        string chatPageId,
        string messageId,
        string newContent)
    {
        using var session = dbContext.OpenSession();
        var chatPage = await session.LoadAsync<ChatPage>(chatPageId);
        if (chatPage == null || chatPage.UserId != userId)
            throw new Exception("Access denied");

        var message = chatPage.History.FirstOrDefault(m => m.Id == messageId);
        if (message == null)
            throw new Exception("Message not found");
        if (message.Sender != "user")
            throw new Exception("Cannot edit this message");

        message.Content = newContent;
        message.Timestamp = DateTime.UtcNow;
        await session.SaveChangesAsync();
        return message;
    }

    // حذف کامل صفحه چت
    public async Task<bool> DeleteChatPageAsync(string userId, string chatPageId)
    {
        using var session = dbContext.OpenSession();
        var chatPage = await session.LoadAsync<ChatPage>(chatPageId);
        if (chatPage == null || chatPage.UserId != userId)
            throw new Exception("Access denied");
        session.Delete(chatPage);
        await session.SaveChangesAsync();
        return true;
    }

    // جستجوی پیام در تاریخچه صفحه چت
    public async Task<List<ChatMessage>> SearchMessagesAsync(string userId, string chatPageId, string? searchText)
    {
        using var session = dbContext.OpenSession();
        var chatPage = await session.LoadAsync<ChatPage>(chatPageId);
        if (chatPage == null || chatPage.UserId != userId)
            throw new Exception("Access denied");

        searchText = searchText?.Trim();
        if (string.IsNullOrEmpty(searchText))
            return new List<ChatMessage>();

        return chatPage.History
            .Where(m => m.Content != null && m.Content.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<UserTokenProfile?> GetUserTokenProfileAsync(string userId)
    {
        using var session = dbContext.OpenSession();
        return await session.LoadAsync<UserTokenProfile>($"UserTokens/{userId}");
    }

    public async Task<UserTokenProfile> RechargeTokensAsync(string userId, int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.");

        using var session = dbContext.OpenSession();
        var userProfile = await session.LoadAsync<UserTokenProfile>($"UserTokens/{userId}");
        if (userProfile == null)
        {
            userProfile = new UserTokenProfile
            {
                Id = $"UserTokens/{userId}",
                UserId = userId,
                TokenBalance = 0,
                InputTokensUsed = 0,
                OutputTokensUsed = 0
            };
            await session.StoreAsync(userProfile);
        }

        userProfile.TokenBalance += amount;
        await session.SaveChangesAsync();

        return userProfile;
    }

    /// <summary>
    ///     دریافت لیست مدل‌های در دسترس از میکروسرویس OpenAI
    /// </summary>
    public async Task<IEnumerable<string>> GetAvailableModelsAsync(string? jwtToken = null)
    {
        using var httpClient = new HttpClient();
        if (!string.IsNullOrEmpty(jwtToken))
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwtToken.Replace("Bearer ", string.Empty));

        var response = await httpClient.GetAsync("https://api-remote-chatgpt-v1.ailand.ai/api/OpenAi/chat/models");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<IEnumerable<string>>(json) ?? [];
    }

    public async Task<List<string>> UploadFilesAsync(
        List<IFormFile> files,
        string? jwtToken,
        CancellationToken cancellationToken = default)
    {
        if (files.Count == 0)
        {
            return [];
        }

        const string uploadUrl = "https://api-remote-chatgpt-v1.ailand.ai/api/OpenAi/chat/files/upload";

        using var httpClient = new HttpClient();

        if (!string.IsNullOrEmpty(jwtToken))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwtToken.Replace("Bearer ", string.Empty));
        }

        using var form = new MultipartFormDataContent();
        foreach (var file in files)
        {
            var streamContent = new StreamContent(file.OpenReadStream());
            form.Add(streamContent, "files", file.FileName);
        }

        try
        {
            var httpResponse = await httpClient.PostAsync(uploadUrl, form, cancellationToken);
            httpResponse.EnsureSuccessStatusCode();
            var jsonResponse = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            var result = JsonConvert.DeserializeObject<UploadFilesResponse>(jsonResponse);

            return result?.FileIds ?? [];
        }
        catch (Exception ex)
        {
            throw new Exception("سرویس آپلود فایل پاسخگو نیست.", ex);
        }
    }

    public async IAsyncEnumerable<string> SendMessageStreamWithFileIdsAsync(string userId, string chatPageId,
        string? userInput,
        IEnumerable<string>? fileIds, string? model, string? jwtToken, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var session = dbContext.OpenSession();
        var tokenCounter = new TokenCounter();

        // ۱. بارگذاری و اعتبارسنجی صفحه چت و پروفایل کاربر
        var chatPage = await session.LoadAsync<ChatPage>(chatPageId, cancellationToken)
                       ?? throw new Exception("صفحه چت یافت نشد یا دسترسی غیرمجاز است.");
        if (chatPage.UserId != userId)
            throw new Exception("دسترسی غیرمجاز.");

        var userProfile = await session.LoadAsync<UserTokenProfile>($"UserTokens/{userId}", cancellationToken)
                          ?? new UserTokenProfile { Id = $"UserTokens/{userId}", UserId = userId };

        // ۲. محاسبه و کسر توکن‌های ورودی
        var inputTokens = tokenCounter.CountTokens(userInput ?? string.Empty);
        if (userProfile.TokenBalance < inputTokens)
            throw new Exception("موجودی توکن برای ارسال پیام کافی نیست.");

        userProfile.InputTokensUsed += inputTokens;
        userProfile.TokenBalance -= inputTokens;

        // ۳. افزودن پیام جدید کاربر به تاریخچه محلی
        chatPage.History.Add(new ChatMessage
        {
            Sender = "user",
            Content = userInput,
            Timestamp = DateTime.UtcNow
        });

        // تغییرات اولیه را ذخیره می‌کنیم
        await session.SaveChangesAsync(cancellationToken);

        using var httpClient = new HttpClient();
        httpClient.Timeout = Timeout.InfiniteTimeSpan;
        if (!string.IsNullOrEmpty(jwtToken))
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwtToken.Replace("Bearer ", string.Empty));

        var requestUrl = $"https://api-remote-chatgpt-v1.ailand.ai/api/OpenAi/chat/stream/files/{model}";

        // ۵. ساخت بدنه درخواست شامل تاریخچه کامل و شناسه‌های فایل
        var remoteRequest = new
        {
            chatPage.History,
            FileIds = fileIds
        };

        var jsonContent = new StringContent(
            JsonConvert.SerializeObject(remoteRequest),
            Encoding.UTF8,
            "application/json");

        using var response = await httpClient.PostAsync(requestUrl, jsonContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        // ۶. پردازش پاسخ استریم (مشابه قبل)
        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(responseStream);
        var responseBuffer = new StringBuilder();

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;

            var rawChunk = line["data: ".Length..];
            if (rawChunk == "[DONE]") break;

            var chunk = ParseStreamChunk(rawChunk); // استفاده از متد کمکی موجود
            if (string.IsNullOrEmpty(chunk)) continue;

            responseBuffer.Append(chunk);
            yield return chunk;
        }

        // ۷. ذخیره پاسخ نهایی و به‌روزرسانی توکن‌های خروجی
        var fullResponseText = responseBuffer.ToString();
        var outputTokens = tokenCounter.CountTokens(fullResponseText);

        if (userProfile.TokenBalance < outputTokens)
        {
            chatPage.History.Add(new ChatMessage
            {
                Sender = "assistant",
                Content = "[ناتوان در ثبت پاسخ کامل به دلیل اتمام توکن‌ها]"
            });
        }
        else
        {
            userProfile.OutputTokensUsed += outputTokens;
            userProfile.TokenBalance -= outputTokens;
            chatPage.History.Add(new ChatMessage
            {
                Sender = "assistant",
                Content = fullResponseText
            });
        }

        await session.SaveChangesAsync(cancellationToken);
    }

    private static string? ParseStreamChunk(string payload)
    {
        using var doc = JsonDocument.Parse(payload);
        var choices = doc.RootElement.GetProperty("choices");
        var delta = choices[0].GetProperty("delta");
        return delta.TryGetProperty("content", out var contentProp) ? contentProp.GetString() : null;
    }
}