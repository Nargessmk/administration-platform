using System.Text;
using System.Text.Json;
using Cloudland.APM.Microservices.ChatGPT.Models;
using Cloudland.APM.Microservices.ChatGPT.Services;
using Cloudland.APM.Models.ChatGPT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cloudland.APM.Microservices.ChatGPT.Controllers;

[ApiController]
[Route("api/[controller]/chat")]
[Authorize]
public class OpenAiController(IOpenAiGateway gateway) : ControllerBase
{
    [HttpPost("{modelName?}")]
    public async Task<IActionResult> Chat(
        [FromRoute] string? modelName,
        [FromBody] IEnumerable<ChatMessage> history)
    {
        var model = string.IsNullOrWhiteSpace(modelName)
            ? "gpt-4.1"
            : modelName;

        var availableModels = await gateway.ListModelsAsync();
        if (!availableModels.Contains(model))
            return BadRequest(new { error = $"Model '{model}' is not available." });

        var response = await gateway.ChatAsync(history, model);
        return Ok(new { result = response });
    }

    [HttpPost("websearch/{modelName?}")]
    public async Task<IActionResult> ChatWithWebSearch(
        [FromRoute] string? modelName,
        [FromBody] IEnumerable<ChatMessage> history)
    {
        var model = string.IsNullOrWhiteSpace(modelName)
            ? "gpt-4.1"
            : modelName;

        var availableModels = await gateway.ListModelsAsync();
        if (!availableModels.Contains(model))
            return BadRequest(new { error = $"Model '{model}' is not available." });

        var response = await gateway.ChatWithWebSearchAsync(history, model);
        return Ok(new { result = response });
    }

    [HttpPost("stream/{modelName?}")]
    public async Task ChatStream(
        [FromRoute] string? modelName,
        [FromBody] IEnumerable<ChatMessage> history)
    {
        var model = string.IsNullOrWhiteSpace(modelName)
            ? "gpt-4.1"
            : modelName;

        var availableModels = await gateway.ListModelsAsync();
        if (!availableModels.Contains(model))
        {
            Response.StatusCode = 400;
            await Response
                .WriteAsync(JsonSerializer.Serialize(new { error = $"Model '{model}' is not available." }));
            return;
        }

        Response.ContentType = "text/event-stream; charset=utf-8";
        var clientToken = HttpContext.RequestAborted;

        await foreach (var chunk in gateway.ChatAsyncStream(history, model, clientToken))
        {
            var data = $"data: {chunk}\n\n";
            var bytes = Encoding.UTF8.GetBytes(data);
            await Response.Body.WriteAsync(bytes, clientToken);
            await Response.Body.FlushAsync(clientToken);
        }
    }

    [HttpPost("websearch/stream/{modelName?}")]
    public async Task ChatWithWebSearchStream(
        [FromRoute] string? modelName,
        [FromBody] IEnumerable<ChatMessage> history)
    {
        var model = string.IsNullOrWhiteSpace(modelName)
            ? "gpt-4.1"
            : modelName;

        var availableModels = await gateway.ListModelsAsync();
        if (!availableModels.Contains(model))
        {
            Response.StatusCode = 400;
            await Response
                .WriteAsync(JsonSerializer.Serialize(new { error = $"Model '{model}' is not available." }));
            return;
        }

        Response.ContentType = "text/event-stream; charset=utf-8";
        var clientToken = HttpContext.RequestAborted;

        await foreach (var chunk in gateway.ChatWithWebSearchAsyncStream(history, model, clientToken))
        {
            var data = $"data: {chunk}\n\n";
            var bytes = Encoding.UTF8.GetBytes(data);
            await Response.Body.WriteAsync(bytes, clientToken);
            await Response.Body.FlushAsync(clientToken);
        }
    }

    [HttpPost("custom")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> ChatCustom([FromForm] ChatCustomRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Model))
            return BadRequest(new { error = "Model is required." });

        if (string.IsNullOrWhiteSpace(req.UserInput) && req.Files.Count == 0)
            return BadRequest(new { error = "Provide userInput or at least one file." });

        var availableModels = await gateway.ListModelsAsync();
        if (!availableModels.Contains(req.Model))
            return BadRequest(new { error = $"Model '{req.Model}' not available." });

        var result = await gateway.ChatWithFilesByModelAsync(
            req.Model,
            req.UserInput,
            req.Files);

        return Ok(new { result });
    }

    [HttpPost("custom/stream")]
    [RequestSizeLimit(100_000_000)]
    public async Task ChatCustomStream([FromForm] ChatCustomRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Model))
        {
            Response.StatusCode = 400;
            await Response
                .WriteAsync(JsonSerializer.Serialize(new { error = "Model is required." }));
            return;
        }

        if (string.IsNullOrWhiteSpace(req.UserInput) && req.Files.Count == 0)
        {
            Response.StatusCode = 400;
            await Response
                .WriteAsync(JsonSerializer.Serialize(new { error = "Provide userInput or at least one file." }));
            return;
        }

        var availableModels = await gateway.ListModelsAsync();
        if (!availableModels.Contains(req.Model))
        {
            Response.StatusCode = 400;
            await Response
                .WriteAsync(JsonSerializer.Serialize(new { error = $"Model '{req.Model}' not available." }));
            return;
        }

        Response.ContentType = "text/event-stream; charset=utf-8";
        var clientToken = HttpContext.RequestAborted;

        await foreach (var chunk in gateway.ChatWithFilesByModelStreamAsync(
                           req.Model,
                           req.UserInput,
                           req.Files,
                           clientToken))
        {
            var data = $"data: {chunk}\n\n";
            var bytes = Encoding.UTF8.GetBytes(data);
            await Response.Body.WriteAsync(bytes, clientToken);
            await Response.Body.FlushAsync(clientToken);
        }
    }

    [HttpGet("models")]
    public async Task<IActionResult> GetModels()
    {
        var models = await gateway.ListModelsAsync();
        return Ok(models);
    }
    
    [HttpPost("files/upload")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> UploadFiles([FromForm] List<IFormFile> files)
    {
        if (files.Count == 0)
        {
            return BadRequest(new { error = "At least one file must be provided." });
        }

        try
        {
            var clientToken = HttpContext.RequestAborted;
            var uploadedFileIds = await gateway.UploadFilesAsync(files, clientToken);
            return Ok(new { fileIds = uploadedFileIds });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred during file upload: {ex.Message}" });
        }
    }
    
    [HttpPost("stream/files/{modelName?}")]
    public async Task ChatStreamWithFiles(
        [FromRoute] string? modelName,
        [FromBody] ChatStreamWithFilesRequest request)
    {
        var model = string.IsNullOrWhiteSpace(modelName)
            ? "gpt-4.1"
            : modelName;

        var availableModels = await gateway.ListModelsAsync();
        if (!availableModels.Contains(model))
        {
            Response.StatusCode = 400;
            await Response.WriteAsync(JsonSerializer.Serialize(new { error = $"Model '{model}' is not available." }));
            return;
        }

        Response.ContentType = "text/event-stream; charset=utf-8";
        var clientToken = HttpContext.RequestAborted;

        await foreach (var chunk in gateway.ChatAsyncStream(request.History, request.FileIds, model, clientToken))
        {
            var data = $"data: {chunk}\n\n";
            var bytes = Encoding.UTF8.GetBytes(data);
            await Response.Body.WriteAsync(bytes, clientToken);
            await Response.Body.FlushAsync(clientToken);
        }
    }
}