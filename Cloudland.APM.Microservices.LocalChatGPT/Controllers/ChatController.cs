using System.Text;
using System.Text.Json;
using Cloudland.APM.Microservices.LocalChatGPT.Models;
using Cloudland.APM.Microservices.LocalChatGPT.Services;
using Cloudland.APM.Models.RequestModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cloudland.APM.Microservices.LocalChatGPT.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]/pages")]
public class ChatController(IChatService chatService) : ControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> GetChatPages()
    {
        var userId = User.Identity?.Name;
        if (userId == null) return BadRequest();
        var pages = await chatService.GetUserChatPagesAsync(userId);
        return Ok(pages);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetChatHistory([FromQuery] string chatPageId)
    {
        var userId = User.Identity?.Name;
        if (userId == null) return BadRequest();
        var history = await chatService.GetChatHistoryAsync(userId, chatPageId);
        return Ok(history);
    }

    [HttpPost("message")]
    public async Task<IActionResult> SendMessage(
        [FromQuery] string chatPageId,
        [FromBody] MessageRequest message)
    {
        var userId = User.Identity?.Name;
        if (userId == null) return BadRequest();

        var jwtToken = Request.Headers.Authorization.ToString();
        var reply = await chatService.SendMessageAsync(
            userId,
            chatPageId,
            message.message,
            jwtToken,
            message.modelName);

        return Ok(reply);
    }

    [HttpPost("message/stream")]
    public async Task SendMessageStream(
        [FromQuery] string chatPageId,
        [FromBody] MessageRequest message)
    {
        var userId = User.Identity?.Name;
        if (userId == null)
        {
            Response.StatusCode = 400;
            await Response.WriteAsync(JsonSerializer.Serialize(new { error = "کاربر یافت نشد." }));
            return;
        }

        var jwtToken = Request.Headers.Authorization.ToString();
        var model = message.modelName;

        Response.ContentType = "text/event-stream; charset=utf-8";

        var cancellationToken = HttpContext.RequestAborted;

        try
        {
            await foreach (var chunk in chatService.SendMessageStreamAsync(
                               userId,
                               chatPageId,
                               model,
                               jwtToken,
                               message,
                               cancellationToken))
            {
                if (string.IsNullOrEmpty(chunk)) continue;

                var eventData = $"data: {chunk}\n\n";
                var eventBytes = Encoding.UTF8.GetBytes(eventData);
                await Response.Body.WriteAsync(eventBytes, cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }

            // ارسال پیام اتمام استریم
            const string endData = "data: [DONE]\n\n";
            var endBytes = Encoding.UTF8.GetBytes(endData);
            await Response.Body.WriteAsync(endBytes, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // مدیریت زمانی که کلاینت ارتباط را قطع کند
        }
        catch (Exception ex)
        {
            Response.StatusCode = 500;
            var errorMessage = $"data: {JsonSerializer.Serialize(new { error = ex.Message })}\n\n";
            var errorBytes = Encoding.UTF8.GetBytes(errorMessage);
            await Response.Body.WriteAsync(errorBytes, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }


    [HttpPost("message/custom")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> SendMessageWithFiles(
        [FromQuery] string chatPageId,
        [FromForm] CustomMessageRequest req)
    {
        var userId = User.Identity?.Name;
        if (userId == null) return BadRequest();
        if (string.IsNullOrWhiteSpace(req.Model))
            return BadRequest(new { error = "Model is required." });
        if (string.IsNullOrWhiteSpace(req.UserInput) && req.Files is { Count: 0 })
            return BadRequest(new { error = "Provide UserInput or at least one file." });

        var jwtToken = Request.Headers.Authorization.ToString();
        var reply = await chatService.SendMessageWithFilesAsync(
            userId,
            chatPageId,
            req.Model,
            req.UserInput,
            req.Files,
            jwtToken
        );
        return Ok(reply);
    }

    // ** جدید: ارسال پیام با فایل‌ها به‌صورت استریم **
    [HttpPost("message/custom/stream")]
    [RequestSizeLimit(100_000_000)]
    public async Task SendMessageWithFilesStream(
        [FromQuery] string chatPageId,
        [FromForm] CustomMessageRequest req)
    {
        var userId = User.Identity?.Name;
        if (userId == null)
        {
            Response.StatusCode = 400;
            await Response.WriteAsync("BadRequest: کاربر یافت نشد.");
            return;
        }

        if (string.IsNullOrWhiteSpace(req.Model))
        {
            Response.StatusCode = 400;
            await Response.WriteAsync(JsonSerializer.Serialize(new { error = "Model is required." }));
            return;
        }

        if (string.IsNullOrWhiteSpace(req.UserInput) && req.Files is { Count: 0 })
        {
            Response.StatusCode = 400;
            await Response.WriteAsync(JsonSerializer.Serialize(new
                { error = "Provide UserInput or at least one file." }));
            return;
        }

        var jwtToken = Request.Headers.Authorization.ToString();
        Response.ContentType = "text/event-stream; charset=utf-8";
        var clientToken = HttpContext.RequestAborted;

        await foreach (var chunk in chatService.SendMessageWithFilesStreamAsync(
                           userId,
                           chatPageId,
                           req.Model,
                           req.UserInput,
                           req.Files,
                           jwtToken).WithCancellation(clientToken))
        {
            var data = $"data: {chunk}\n\n";
            var bytes = Encoding.UTF8.GetBytes(data);
            await Response.Body.WriteAsync(bytes, 0, bytes.Length, clientToken);
            await Response.Body.FlushAsync(clientToken);
        }
    }

    [HttpDelete("message")]
    public async Task<IActionResult> DeleteMessage(
        [FromQuery] string chatPageId,
        [FromQuery] string messageId)
    {
        var userId = User.Identity?.Name;
        var result = userId != null &&
                     await chatService.DeleteMessageAsync(userId, chatPageId, messageId);
        return result ? Ok() : NotFound();
    }

    [HttpPut("message")]
    public async Task<IActionResult> EditMessage(
        [FromQuery] string chatPageId,
        [FromQuery] string messageId,
        [FromBody] MessageRequest newContent)
    {
        var userId = User.Identity?.Name;
        if (userId == null) return BadRequest();
        var message = await chatService.EditMessageAsync(
            userId,
            chatPageId,
            messageId,
            newContent.message);
        return Ok(message);
    }

    [HttpPost("")]
    public async Task<IActionResult> CreatePage()
    {
        var userId = User.Identity?.Name;
        if (userId == null) return BadRequest();
        var page = await chatService.GetOrCreateChatPageAsync(userId);
        return Ok(page);
    }

    [HttpDelete("")]
    public async Task<IActionResult> DeleteChatPage([FromQuery] string chatPageId)
    {
        var userId = User.Identity?.Name;
        var result = userId != null &&
                     await chatService.DeleteChatPageAsync(userId, chatPageId);
        return result ? Ok() : NotFound();
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchMessages(
        [FromQuery] string chatPageId,
        [FromQuery] string? query)
    {
        var userId = User.Identity?.Name;
        if (userId == null) return BadRequest();
        var messages = await chatService.SearchMessagesAsync(
            userId,
            chatPageId,
            query);
        return Ok(messages);
    }

    [HttpGet("tokens")]
    public async Task<IActionResult> GetTokenProfile()
    {
        var userId = User.Identity?.Name;
        if (userId == null) return BadRequest();

        var userProfile = await chatService.GetUserTokenProfileAsync(userId);
        if (userProfile == null)
            return NotFound("پروفایل توکن یافت نشد.");

        return Ok(new
        {
            userProfile.TokenBalance,
            userProfile.InputTokensUsed,
            userProfile.OutputTokensUsed
        });
    }

    [HttpPost("tokens/recharge")]
    [Authorize("admin")]
    public async Task<IActionResult> RechargeTokens([FromBody] RechargeRequest req)
    {
        var userId = User.Identity?.Name;
        if (userId == null) return BadRequest();

        var userProfile = await chatService.RechargeTokensAsync(userId, req.Amount);
        return Ok(new
        {
            userProfile.TokenBalance,
            userProfile.InputTokensUsed,
            userProfile.OutputTokensUsed
        });
    }

    [HttpPost("files/upload")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> UploadFiles([FromForm] List<IFormFile> files)
    {
        var userId = User.Identity?.Name;
        if (userId == null)
        {
            return BadRequest(new { error = "کاربر یافت نشد." });
        }

        if (files.Count == 0)
        {
            return BadRequest(new { error = "حداقل یک فایل باید ارسال شود." });
        }

        var jwtToken = Request.Headers.Authorization.ToString();
        var cancellationToken = HttpContext.RequestAborted;

        try
        {
            var fileIds = await chatService.UploadFilesAsync(files, jwtToken, cancellationToken);
            return Ok(new { fileIds });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("models")]
    public async Task<IActionResult> GetAvailableModels()
    {
        var jwtToken = Request.Headers.Authorization.ToString();
        var models = await chatService.GetAvailableModelsAsync(jwtToken);
        return Ok(models);
    }

    [HttpPost("message/stream/with-ids")]
    public async Task SendMessageStreamWithFileIds(
        [FromQuery] string chatPageId,
        [FromBody] MessageWithFileIdsRequest request)
    {
        var userId = User.Identity?.Name;
        if (userId == null)
        {
            Response.StatusCode = 400;
            await Response.WriteAsync(JsonSerializer.Serialize(new { error = "کاربر یافت نشد." }));
            return;
        }

        var fileIdList = request.FileIds?.ToList();
        if (string.IsNullOrWhiteSpace(request.UserInput) && (fileIdList == null || fileIdList.Count == 0))
        {
            Response.StatusCode = 400;
            await Response.WriteAsync(JsonSerializer.Serialize(new
                { error = "حداقل یک پیام متنی یا شناسه فایل باید ارائه شود." }));
            return;
        }

        var jwtToken = Request.Headers.Authorization.ToString();
        Response.ContentType = "text/event-stream; charset=utf-8";
        var cancellationToken = HttpContext.RequestAborted;

        try
        {
            await foreach (var chunk in chatService.SendMessageStreamWithFileIdsAsync(
                               userId,
                               chatPageId,
                               request.UserInput,
                               fileIdList,
                               request.ModelName,
                               jwtToken,
                               cancellationToken))
            {
                if (string.IsNullOrEmpty(chunk)) continue;

                var eventData = $"data: {chunk}\n\n";
                var eventBytes = Encoding.UTF8.GetBytes(eventData);
                await Response.Body.WriteAsync(eventBytes, cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }

            const string endData = "data: [DONE]\n\n";
            var endBytes = Encoding.UTF8.GetBytes(endData);
            await Response.Body.WriteAsync(endBytes, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
           
        }
        catch (Exception ex)
        {
            Response.StatusCode = 500;
            var errorMessage = $"data: {JsonSerializer.Serialize(new { error = ex.Message })}\n\n";
            var errorBytes = Encoding.UTF8.GetBytes(errorMessage);
            await Response.Body.WriteAsync(errorBytes, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}