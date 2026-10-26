using System.Net;
using Asp.Versioning;
using AutoMapper;
using MassTransit;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.Controller.Base;
using Cloudland.APM.Microservices.AppManagement.CQRS.Commands;
using Cloudland.APM.Microservices.AppManagement.CQRS.Queries;
using Cloudland.APM.Models.ResultModel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Nest;

namespace Cloudland.APM.Microservices.AppManagement.Controller;

[ApiVersion("1.0")]
[Route("api/v1/[controller]")]
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Authorize]
public class CalendarController(
    IMediator mediator,
    IMapper mapper,
    IDistributedCache distributedCache,
    IElasticClient elasticClient,
    IPublishEndpoint publishEndpoint) :
    BaseController<CalendarDTO, Guid>(mediator, mapper, distributedCache, elasticClient, publishEndpoint)
{
    /// <summary>
    /// Generate and store Persian calendar for a specific year
    /// </summary>
    /// <param name="year">Persian year (e.g., 1403)</param>
    /// <param name="parentId">Parent Constant ID for calendar entries</param>
    /// <param name="nodeTypeId">Optional NodeTypeId for calendar constants</param>
    /// <returns>Number of calendar entries created</returns>
    [HttpPost("Generate/{year:int}/{parentId:guid}")]
    [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<int>> GenerateCalendar(int year, Guid parentId, [FromQuery] Guid? nodeTypeId = null)
    {
        if (year < 1300 || year > 1500)
        {
            return BadRequest("Year must be between 1300 and 1500");
        }

        var result = await _mediator.Send(new GenerateCalendarCommand(year, parentId, nodeTypeId));
        return Ok(result);
    }

    /// <summary>
    /// Add a new calendar entry
    /// </summary>
    /// <param name="calendar">Calendar data</param>
    /// <returns>Created calendar entry</returns>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CalendarDTO), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AddCalendar([FromBody] CalendarDTO calendar)
    {
        var calendarToReturn = await _mediator.Send(new AddCalendarCommand(calendar));
        if (calendarToReturn == null) return BadRequest("Calendar entry already exists for this date or invalid data");
        return Ok(calendarToReturn);
    }

    /// <summary>
    /// Update an existing calendar entry
    /// </summary>
    /// <param name="calendar">Calendar data with ID</param>
    /// <returns>Updated calendar entry</returns>
    [HttpPut]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CalendarDTO), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateCalendar([FromBody] CalendarDTO calendar)
    {
        try
        {
            var calendarToReturn = await _mediator.Send(new UpdateCalendarCommand(calendar));
            return Ok(calendarToReturn);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Delete a calendar entry
    /// </summary>
    /// <param name="id">Calendar entry ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id:guid}")]
    [Produces("application/json")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteCalendar(Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteCalendarCommand(id));
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get a calendar entry by ID
    /// </summary>
    /// <param name="id">Calendar entry ID</param>
    /// <returns>Calendar entry</returns>
    [HttpGet("ById/{id:guid}")]
    [ProducesResponseType(typeof(CalendarDTO), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> GetCalendarById(Guid id)
    {
        var calendar = await _mediator.Send(new GetCalendarByIdQuery(id));
        if (calendar == null) return NotFound();
        return Ok(calendar);
    }

    /// <summary>
    /// Get paginated calendar entries for a specific Persian year
    /// </summary>
    /// <param name="year">Persian year (e.g., 1404)</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated calendar entries for the specified year</returns>
    [HttpGet("Paging/{year:int}/{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType(typeof(PagedResult<CalendarDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult> GetCalendarPaging(int year, int pageNumber, int pageSize)
    {
        if (year < 1300 || year > 1500)
        {
            return BadRequest("Year must be between 1300 and 1500");
        }

        var result = await _mediator.Send(new GetCalendarPagingQuery(year, pageNumber, pageSize));
        return Ok(result);
    }
}

