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
public class ReservationRequestPersonController(
    IMediator mediator,
    IMapper mapper,
    IDistributedCache distributedCache,
    IElasticClient elasticClient,
    IPublishEndpoint publishEndpoint) :
    BaseController<ReservationRequestPersonDTO, Guid>(mediator, mapper, distributedCache, elasticClient, publishEndpoint)
{
    private readonly ReservationRequestPersonDTOValidator _validator = new();

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResultDTO<ReservationRequestPersonDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestPersonById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestPersonByIdQuery(id));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("paging/{pageNumber:int:min(1)}/{pageSize:int:min(1)}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ReservationRequestPersonDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestPersonPaging(int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestPersonPagingQuery(pageNumber, pageSize));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResultDTO<ReservationRequestPersonDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> CreateReservationRequestPerson([FromBody] ReservationRequestPersonDTO ReservationRequestPerson)
    {
        try
        {
            var validationResult = ReservationRequestPerson.ValidateAndGetResult(_validator);
            if (!validationResult.IsSuccess)
            {
                return BadRequest(validationResult);
            }

            var result = await _mediator.Send(new AddReservationRequestPersonCommand(ReservationRequestPerson));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut]
    [ProducesResponseType(typeof(ResultDTO<ReservationRequestPersonDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> UpdateReservationRequestPerson([FromBody] ReservationRequestPersonDTO ReservationRequestPerson)
    {
        try
        {
            var validationResult = ReservationRequestPerson.ValidateAndGetResult(_validator);
            if (!validationResult.IsSuccess)
            {
                return BadRequest(validationResult);
            }

            var result = await _mediator.Send(new UpdateReservationRequestPersonCommand(ReservationRequestPerson));
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
    public virtual async Task<IActionResult> DeleteReservationRequestPerson(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteReservationRequestPersonCommand(id));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // View endpoints
    [HttpGet("view")]
    [ProducesResponseType(typeof(ResultDTO<List<ReservationRequestPersonViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestPersonViewAll()
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestPersonViewAllQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/paging/{pageNumber:int:min(1)}/{pageSize:int:min(1)}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ReservationRequestPersonViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestPersonViewPaging(int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestPersonViewAllPagingQuery(pageNumber, pageSize));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/{id:guid}")]
    [ProducesResponseType(typeof(ResultDTO<ReservationRequestPersonViewDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestPersonViewById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestPersonViewByIdQuery(id));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/by-reservation-request-header/{reservationRequestId:guid}")]
    [ProducesResponseType(typeof(ResultDTO<List<ReservationRequestPersonViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestPersonViewByReservationRequestHeaderId(Guid reservationRequestId)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestPersonViewByReservationRequestHeaderIdQuery(reservationRequestId));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/by-reservation-request-header/{reservationRequestId:guid}/paging/{pageNumber:int:min(1)}/{pageSize:int:min(1)}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ReservationRequestPersonViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestPersonViewByReservationRequestHeaderIdPaging(Guid reservationRequestId, int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestPersonViewByReservationRequestHeaderIdPagingQuery(reservationRequestId, pageNumber, pageSize));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}


