using Asp.Versioning;
using Cloudland.APM.Microservices.AppManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cloudland.APM.Microservices.AppManagement.Controller;

[ApiVersion("1.0")]
[Route("api/v1/[controller]")]
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Authorize]
public class ReindexController : ControllerBase
{
    private readonly IReindexService _reindexService;
    private readonly ILogger<ReindexController> _logger;

    public ReindexController(IReindexService reindexService, ILogger<ReindexController> logger)
    {
        _reindexService = reindexService;
        _logger = logger;
    }
    
    [HttpGet("views")]
    public async Task<ActionResult<List<string>>> GetAvailableViews()
    {
        try
        {
            var views = await _reindexService.GetAvailableViewNamesAsync();
            return Ok(views);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available views");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("reindex/{viewName}")]
    public async Task<ActionResult<ReindexResult>> ReindexView(string viewName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting reindex for view: {ViewName}", viewName);
            
            var result = await _reindexService.ReindexByViewNameAsync(viewName, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Reindex completed successfully for view: {ViewName}. Processed: {Processed}, Errors: {Errors}", 
                    viewName, result.ProcessedCount, result.ErrorCount);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Reindex failed for view: {ViewName}. Error: {Error}", viewName, result.Message);
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during reindex for view: {ViewName}", viewName);
            return StatusCode(500, new ReindexResult
            {
                IsSuccess = false,
                Message = $"Internal server error: {ex.Message}",
                ProcessedCount = 0,
                ErrorCount = 0
            });
        }
    }

    [HttpPost("reindex-all")]
    public async Task<ActionResult<ReindexResult>> ReindexAll(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting reindex for all views");
            
            var result = await _reindexService.ReindexAllAsync(cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Reindex completed successfully for all views. Processed: {Processed}, Errors: {Errors}", 
                    result.ProcessedCount, result.ErrorCount);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Reindex failed for all views. Error: {Error}", result.Message);
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during reindex for all views");
            return StatusCode(500, new ReindexResult
            {
                IsSuccess = false,
                Message = $"Internal server error: {ex.Message}",
                ProcessedCount = 0,
                ErrorCount = 0
            });
        }
    }
}
