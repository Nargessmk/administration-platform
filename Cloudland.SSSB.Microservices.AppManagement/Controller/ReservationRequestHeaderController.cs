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
using Cloudland.SSSB.Models.View;
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
public class ReservationRequestHeaderController(
    IMediator mediator,
    IMapper mapper,
    IDistributedCache distributedCache,
    IElasticClient elasticClient,
    IPublishEndpoint publishEndpoint) :
    BaseController<ReservationRequestHeaderDTO, Guid>(mediator, mapper, distributedCache, elasticClient, publishEndpoint)
{
    private readonly ReservationRequestHeaderDTOValidator _validator = new();

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResultDTO<ReservationRequestHeaderDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestHeaderById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestHeaderByIdQuery(id));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("paging/{pageNumber:int:min(1)}/{pageSize:int:min(1)}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ReservationRequestHeaderDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestHeaderPaging(int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestHeaderPagingQuery(pageNumber, pageSize));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResultDTO<ReservationRequestHeaderViewDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> CreateReservationRequestHeader([FromBody] ReservationRequestHeaderDTO ReservationRequestHeader)
    {
        try
        {
            var validationResult = ReservationRequestHeader.ValidateAndGetResult(_validator);
            if (!validationResult.IsSuccess)
            {
                return BadRequest(validationResult);
            }

            var result = await _mediator.Send(new AddReservationRequestHeaderCommand(ReservationRequestHeader));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut]
    [ProducesResponseType(typeof(ResultDTO<ReservationRequestHeaderDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> UpdateReservationRequestHeader([FromBody] ReservationRequestHeaderDTO ReservationRequestHeader)
    {
        try
        {
            var validationResult = ReservationRequestHeader.ValidateAndGetResult(_validator);
            if (!validationResult.IsSuccess)
            {
                return BadRequest(validationResult);
            }

            var result = await _mediator.Send(new UpdateReservationRequestHeaderCommand(ReservationRequestHeader));
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
    public virtual async Task<IActionResult> DeleteReservationRequestHeader(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteReservationRequestHeaderCommand(id));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // View endpoints
    [HttpGet("view")]
    [ProducesResponseType(typeof(ResultDTO<List<ReservationRequestHeaderViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestHeaderViewAll()
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestHeaderViewAllQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/paging/{pageNumber:int:min(1)}/{pageSize:int:min(1)}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ReservationRequestHeaderViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestHeaderViewPaging(int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestHeaderViewAllPagingQuery(pageNumber, pageSize));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/{id:guid}")]
    [ProducesResponseType(typeof(ResultDTO<ReservationRequestHeaderViewDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestHeaderViewById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestHeaderViewByIdQuery(id));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/count/date/{**persianDate}")]
    [ProducesResponseType(typeof(ResultDTO<int>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestHeaderViewCountByDate(string persianDate)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestHeaderViewCountByDateQuery(persianDate));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/count/today")]
    [ProducesResponseType(typeof(ResultDTO<int>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestHeaderViewCountToday()
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestHeaderViewCountTodayQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/count/date/{persianDate}/user")]
    [ProducesResponseType(typeof(ResultDTO<int>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestHeaderViewCountByDateForUser(string persianDate)
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestHeaderViewCountByDateForUserQuery(persianDate));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/count/today/user")]
    [ProducesResponseType(typeof(ResultDTO<int>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetReservationRequestHeaderViewCountTodayForUser()
    {
        try
        {
            var result = await _mediator.Send(new GetReservationRequestHeaderViewCountTodayForUserQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}


