using Asp.Versioning;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cloudland.SSSB.Microservices.AppManagement.Controller;

[ApiVersion("1.0")]
[Route("api/v1/[controller]")]
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Authorize]
public class ReindexController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReindexController> _logger;

    public ReindexController(IMediator mediator, ILogger<ReindexController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("views")]
    public async Task<ActionResult<List<string>>> GetAvailableViews()
    {
        try
        {
            var views = await _mediator.Send(new GetAvailableViewsCommand());
            return Ok(views);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available views");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPost("view/{viewName}")]
    public async Task<ActionResult<ReindexResult>> ReindexView(string viewName)
    {
        try
        {
            _logger.LogInformation("Starting reindex for view: {ViewName}", viewName);
            
            var result = await _mediator.Send(new ReindexByViewCommand(viewName));
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Reindex completed for {ViewName}. Processed: {Processed}, Errors: {Errors}", 
                    viewName, result.ProcessedCount, result.ErrorCount);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Reindex failed for {ViewName}: {Message}", viewName, result.Message);
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during reindex for view: {ViewName}", viewName);
            return StatusCode(500, new ReindexResult 
            { 
                IsSuccess = false, 
                Message = $"Internal server error: {ex.Message}" 
            });
        }
    }
    
    [HttpPost("all")]
    public async Task<ActionResult<ReindexResult>> ReindexAll()
    {
        try
        {
            _logger.LogInformation("Starting reindex for all views");
            
            var result = await _mediator.Send(new ReindexAllCommand());
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Reindex completed for all views. Processed: {Processed}, Errors: {Errors}", 
                    result.ProcessedCount, result.ErrorCount);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Reindex failed for all views: {Message}", result.Message);
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during reindex for all views");
            return StatusCode(500, new ReindexResult 
            { 
                IsSuccess = false, 
                Message = $"Internal server error: {ex.Message}" 
            });
        }
    }

    [HttpGet("status")]
    public async Task<ActionResult<object>> GetReindexStatus()
    {
        try
        {
            // This could be enhanced to check actual Elasticsearch index status
            var views = await _mediator.Send(new GetAvailableViewsCommand());
            
            return Ok(new
            {
                AvailableViews = views,
                Message = "Use POST endpoints to trigger reindexing",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reindex status");
            return StatusCode(500, "Internal server error");
        }
    }
}
