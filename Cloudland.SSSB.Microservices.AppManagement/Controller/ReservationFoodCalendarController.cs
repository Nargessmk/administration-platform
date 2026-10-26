using System.Net;
using Asp.Versioning;
using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.Validation;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using Cloudland.SSSB.Microservices.AppManagement.Controller.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.DTO.ViewDTO;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Nest;

namespace Cloudland.SSSB.Microservices.AppManagement.Controller;

[ApiVersion("1.0")]
[Route("api/v1/[controller]")]
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Authorize]
public class ReservationFoodCalendarController(
    IMediator mediator,
    IMapper mapper,
    IDistributedCache distributedCache,
    IElasticClient elasticClient,
    IPublishEndpoint publishEndpoint) :
    BaseController<ReservationFoodCalendarDTO, Guid>(mediator, mapper, distributedCache, elasticClient, publishEndpoint)
{
    private readonly ReservationFoodCalendarDTOValidator _validator = new();

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResultDTO<ReservationFoodCalendarDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationFoodCalendarById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationFoodCalendarByIdQuery(id));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("paging/{pageNumber:int:min(1)}/{pageSize:int:min(1)}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ReservationFoodCalendarDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationFoodCalendarPaging(int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationFoodCalendarPagingQuery(pageNumber, pageSize));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResultDTO<ReservationFoodCalendarDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> CreateReservationFoodCalendar([FromBody] ReservationFoodCalendarDTO ReservationFoodCalendar)
    {
        try
        {
            var validationResult = ReservationFoodCalendar.ValidateAndGetResult(_validator);
            if (!validationResult.IsSuccess)
            {
                return BadRequest(validationResult);
            }

            var result = await _mediator.Send(new AddReservationFoodCalendarCommand(ReservationFoodCalendar));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut]
    [ProducesResponseType(typeof(ResultDTO<ReservationFoodCalendarDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> UpdateReservationFoodCalendar([FromBody] ReservationFoodCalendarDTO ReservationFoodCalendar)
    {
        try
        {
            var validationResult = ReservationFoodCalendar.ValidateAndGetResult(_validator);
            if (!validationResult.IsSuccess)
            {
                return BadRequest(validationResult);
            }

            var result = await _mediator.Send(new UpdateReservationFoodCalendarCommand(ReservationFoodCalendar));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ResultDTO<int>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> DeleteReservationFoodCalendar(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteReservationFoodCalendarCommand(id));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // View endpoints
    [HttpGet("view")]
    [ProducesResponseType(typeof(ResultDTO<List<ReservationFoodCalendarViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationFoodCalendarViewAll()
    {
        try
        {
            var result = await _mediator.Send(new GetReservationFoodCalendarViewAllQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/paging/{pageNumber:int:min(1)}/{pageSize:int:min(1)}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ReservationFoodCalendarViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationFoodCalendarViewPaging(int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationFoodCalendarViewAllPagingQuery(pageNumber, pageSize));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/{id:guid}")]
    [ProducesResponseType(typeof(ResultDTO<ReservationFoodCalendarViewDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationFoodCalendarViewById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationFoodCalendarViewByIdQuery(id));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/by-reservation-request-header/{reservationRequestId:guid}")]
    [ProducesResponseType(typeof(ResultDTO<List<ReservationFoodCalendarViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationFoodCalendarViewByReservationRequestHeaderId(Guid reservationRequestId)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationFoodCalendarViewByReservationRequestHeaderIdQuery(reservationRequestId));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/by-reservation-request-header/{reservationRequestId:guid}/paging/{pageNumber:int:min(1)}/{pageSize:int:min(1)}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ReservationFoodCalendarViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationFoodCalendarViewByReservationRequestHeaderIdPaging(Guid reservationRequestId, int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationFoodCalendarViewByReservationRequestHeaderIdPagingQuery(reservationRequestId, pageNumber, pageSize));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("bulk-reserve")]
    [ProducesResponseType(typeof(ResultDTO<int>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> BulkCreateFoodReservation([FromBody] List<ShiftWorkerRequestDTO> requests)
    {
        try
        {
            if (requests == null || !requests.Any())
            {
                return BadRequest(new { message = "لیست درخواست‌ها نمی‌تواند خالی باشد" });
            }

            var result = await _mediator.Send(new BulkCreateFoodReservationCommand(requests));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("shift-worker-requests/{reservationRequestHeaderId:guid}")]
    [ProducesResponseType(typeof(ResultDTO<List<ShiftWorkerRequestDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetShiftWorkerRequestsByReservationRequestHeaderId(Guid reservationRequestHeaderId)
    {
        try
        {
            var result = await _mediator.Send(new GetShiftWorkerRequestsByReservationRequestHeaderIdQuery(reservationRequestHeaderId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
