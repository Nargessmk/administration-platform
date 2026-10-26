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
public class ReservationRequestLetterController(
    IMediator mediator,
    IMapper mapper,
    IDistributedCache distributedCache,
    IElasticClient elasticClient,
    IPublishEndpoint publishEndpoint) :
    BaseController<ReservationRequestLetterDTO, Guid>(mediator, mapper, distributedCache, elasticClient, publishEndpoint)
{
    private readonly ReservationRequestLetterDTOValidator _validator = new();

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResultDTO<ReservationRequestLetterDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestLetterById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestLetterByIdQuery(id));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("paging/{pageNumber:int:min(1)}/{pageSize:int:min(1)}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ReservationRequestLetterDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestLetterPaging(int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestLetterPagingQuery(pageNumber, pageSize));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResultDTO<ReservationRequestLetterDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> CreateReservationRequestLetter([FromBody] ReservationRequestLetterDTO reservationRequestLetter)
    {
        try
        {
            var validationResult = reservationRequestLetter.ValidateAndGetResult(_validator);
            if (!validationResult.IsSuccess)
            {
                return BadRequest(validationResult);
            }

            var result = await _mediator.Send(new AddReservationRequestLetterCommand(reservationRequestLetter));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut]
    [ProducesResponseType(typeof(ResultDTO<ReservationRequestLetterDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> UpdateReservationRequestLetter([FromBody] ReservationRequestLetterDTO reservationRequestLetter)
    {
        try
        {
            var validationResult = reservationRequestLetter.ValidateAndGetResult(_validator);
            if (!validationResult.IsSuccess)
            {
                return BadRequest(validationResult);
            }

            var result = await _mediator.Send(new UpdateReservationRequestLetterCommand(reservationRequestLetter));
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
    public virtual async Task<IActionResult> DeleteReservationRequestLetter(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteReservationRequestLetterCommand(id));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // View endpoints
    [HttpGet("view")]
    [ProducesResponseType(typeof(ResultDTO<List<ReservationRequestLetterViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestLetterViewAll()
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestLetterViewAllQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/paging/{pageNumber:int:min(1)}/{pageSize:int:min(1)}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ReservationRequestLetterViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestLetterViewPaging(int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestLetterViewAllPagingQuery(pageNumber, pageSize));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/{id:guid}")]
    [ProducesResponseType(typeof(ResultDTO<ReservationRequestLetterViewDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestLetterViewById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestLetterViewByIdQuery(id));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/by-reservation-request-header/{reservationRequestId:guid}")]
    [ProducesResponseType(typeof(ResultDTO<ReservationRequestLetterViewDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestLetterViewByReservationRequestHeaderId(Guid reservationRequestId)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestLetterViewByReservationRequestHeaderIdQuery(reservationRequestId));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/by-reservation-request-header/{reservationRequestId:guid}/paging/{pageNumber:int:min(1)}/{pageSize:int:min(1)}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ReservationRequestLetterViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestLetterViewByReservationRequestHeaderIdPaging(Guid reservationRequestId, int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestLetterViewByReservationRequestHeaderIdPagingQuery(reservationRequestId, pageNumber, pageSize));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}


