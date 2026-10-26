using System.Net;
using Asp.Versioning;
using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.Validation;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using Cloudland.SSSB.Microservices.AppManagement.Controller.Base;
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
public class ContractFoodCalendarController(
    IMediator mediator,
    IMapper mapper,
    IDistributedCache distributedCache,
    IElasticClient elasticClient,
    IPublishEndpoint publishEndpoint) :
    BaseController<ContractFoodCalendarDTO, Guid>(mediator, mapper, distributedCache, elasticClient, publishEndpoint)
{
    private readonly ContractFoodCalendarDTOValidator _validator = new();

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResultDTO<ContractFoodCalendarDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetContractFoodCalendarById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetContractFoodCalendarByIdQuery(id));
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("Paging/{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ContractFoodCalendarDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetPagingContractFoodCalendars(int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetContractFoodCalendarPagingQuery(pageNumber, pageSize));
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResultDTO<ContractFoodCalendarDTO>), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> CreateContractFoodCalendar([FromBody] ContractFoodCalendarDTO dto)
    {
        try
        {
            var validationResult = dto.ValidateAndGetResult(_validator);
            if (!validationResult.IsSuccess)
            {
                return BadRequest(validationResult);
            }

            var command = new AddContractFoodCalendarCommand(dto);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetContractFoodCalendarById), new { id = result.Data?.Id }, result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpPut]
    [ProducesResponseType(typeof(ResultDTO<ContractFoodCalendarDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> UpdateContractFoodCalendar([FromBody] ContractFoodCalendarDTO dto)
    {
        try
        {
            var validationResult = dto.ValidateAndGetResult(_validator);
            if (!validationResult.IsSuccess)
            {
                return BadRequest(validationResult);
            }

            var command = new UpdateContractFoodCalendarCommand(dto);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
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
    public virtual async Task<IActionResult> DeleteContractFoodCalendar(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteContractFoodCalendarCommand(id));
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("by-contract/{contractId:guid}/food/{foodId:guid}/location/{locationId:guid}/meal/{mealId:guid}/date/{date}")]
    [ProducesResponseType(typeof(ResultDTO<int>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> DeleteContractFoodCalendarByContractFoodLocationIdAndContractIdAndMealId(Guid contractId, Guid foodId, Guid locationId, Guid mealId, string date)
    {
        try
        {
            var result = await _mediator.Send(new DeleteContractFoodCalendarByContractFoodLocationIdAndContractIdAndMealIdCommand(contractId, foodId, locationId, mealId, date));
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // View endpoints
    [HttpGet("View/All")]
    [ProducesResponseType(typeof(ResultDTO<List<ContractFoodCalendarViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetContractFoodCalendarViewAll()
    {
        try
        {
            var result = await _mediator.Send(new GetContractFoodCalendarViewAllQuery());
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("View/Paging/{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetContractFoodCalendarViewPaging(int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetContractFoodCalendarViewAllPagingQuery(pageNumber, pageSize));
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("View/{id:guid}")]
    [ProducesResponseType(typeof(ResultDTO<ContractFoodCalendarViewDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetContractFoodCalendarViewById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetContractFoodCalendarViewByIdQuery(id));
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("View/by-contract-food-location/{contractFoodLocationId:guid}")]
    [ProducesResponseType(typeof(ResultDTO<List<ContractFoodCalendarViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetContractFoodCalendarViewByContractFoodLocationId(Guid contractFoodLocationId)
    {
        try
        {
            var result = await _mediator.Send(new GetContractFoodCalendarViewByContractFoodLocationIdQuery(contractFoodLocationId));
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("View/by-contract-food-location/{contractFoodLocationId:guid}/paging/{pageNumber:int:min(1)}/{pageSize:int:min(1)}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetContractFoodCalendarViewByContractFoodLocationIdPaging(Guid contractFoodLocationId, int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetContractFoodCalendarViewByContractFoodLocationIdPagingQuery(contractFoodLocationId, pageNumber, pageSize));
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("View/by-location/{locationId:guid}")]
    [ProducesResponseType(typeof(ResultDTO<List<ContractFoodCalendarViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetContractFoodCalendarViewByLocationId(Guid locationId)
    {
        try
        {
            var result = await _mediator.Send(new GetContractFoodCalendarViewByLocationIdQuery(locationId));
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("View/by-location/{locationId:guid}/paging/{pageNumber:int:min(1)}/{pageSize:int:min(1)}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetContractFoodCalendarViewByLocationIdPaging(Guid locationId, int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetContractFoodCalendarViewByLocationIdPagingQuery(locationId, pageNumber, pageSize));
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("View/by-location-and-contract/{locationId:guid}/{contractId:guid}")]
    [ProducesResponseType(typeof(ResultDTO<List<ContractFoodCalendarViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetContractFoodCalendarViewByLocationIdAndContractId(Guid locationId, Guid contractId)
    {
        try
        {
            var result = await _mediator.Send(new GetContractFoodCalendarViewByLocationIdAndContractIdQuery(locationId, contractId));
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("View/by-location-and-contract/{locationId:guid}/{contractId:guid}/paging/{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetContractFoodCalendarViewByLocationIdAndContractIdPaging(Guid locationId, Guid contractId, int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetContractFoodCalendarViewByLocationIdAndContractIdPagingQuery(locationId, contractId, pageNumber, pageSize));
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("View/by-location-and-contract/{locationId:guid}/{contractId:guid}/month/{month}")]
    [ProducesResponseType(typeof(ResultDTO<List<ContractFoodCalendarViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetContractFoodCalendarViewByLocationIdAndContractIdAndMonth(Guid locationId, Guid contractId, string month)
    {
        try
        {
            var result = await _mediator.Send(new GetContractFoodCalendarViewByLocationIdAndContractIdAndMonthQuery(locationId, contractId, month));
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("View/by-location-and-contract/{locationId:guid}/{contractId:guid}/month/{month}/paging/{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetContractFoodCalendarViewByLocationIdAndContractIdAndMonthPaging(Guid locationId, Guid contractId, string month, int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetContractFoodCalendarViewByLocationIdAndContractIdAndMonthPagingQuery(locationId, contractId, month, pageNumber, pageSize));
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("View/by-location/{locationId:guid}/month/{month}")]
    [ProducesResponseType(typeof(ResultDTO<List<ContractFoodCalendarViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetContractFoodCalendarViewByLocationIdAndMonth(Guid locationId, string month)
    {
        try
        {
            var result = await _mediator.Send(new GetContractFoodCalendarViewByLocationIdAndMonthQuery(locationId, month));
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("View/by-location/{locationId:guid}/month/{month}/paging/{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetContractFoodCalendarViewByLocationIdAndMonthPaging(Guid locationId, string month, int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetContractFoodCalendarViewByLocationIdAndMonthPagingQuery(locationId, month, pageNumber, pageSize));
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("View/by-location-and-contract/{locationId:guid}/{contractId:guid}/month/{month}/meal/{mealId:guid}")]
    [ProducesResponseType(typeof(ResultDTO<List<ContractFoodCalendarViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetContractFoodCalendarViewByLocationIdAndContractIdAndMonthAndMealId(Guid locationId, Guid contractId, string month, Guid mealId)
    {
        try
        {
            var result = await _mediator.Send(new GetContractFoodCalendarViewByLocationIdAndContractIdAndMonthAndMealIdQuery(locationId, contractId, month, mealId));
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("View/by-location-and-contract/{locationId:guid}/{contractId:guid}/month/{month}/meal/{mealId:guid}/paging/{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetContractFoodCalendarViewByLocationIdAndContractIdAndMonthAndMealIdPaging(Guid locationId, Guid contractId, string month, Guid mealId, int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetContractFoodCalendarViewByLocationIdAndContractIdAndMonthAndMealIdPagingQuery(locationId, contractId, month, mealId, pageNumber, pageSize));
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}


