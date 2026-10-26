using System.Net;
using Asp.Versioning;
using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.Validation;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.DTO.ViewDTO;
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
public class AccountNumberController(
    IMediator mediator,
    IMapper mapper,
    IDistributedCache distributedCache,
    IElasticClient elasticClient,
    IPublishEndpoint publishEndpoint) :
    BaseController<AccountNumberDTO, Guid>(mediator, mapper, distributedCache, elasticClient, publishEndpoint)
{
    private readonly AccountNumberDTOValidator _validator = new();

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResultDTO<AccountNumberDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetAccountNumberById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetAccountNumberByIdQuery(id));
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
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<AccountNumberDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetPagingAccountNumbers(int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetAccountNumberPagingQuery(pageNumber, pageSize));
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
    [ProducesResponseType(typeof(ResultDTO<AccountNumberDTO>), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> CreateAccountNumber([FromBody] AccountNumberDTO AccountNumber)
    {
        
        try
        {
            // Fluent Validation
            var validationResult = AccountNumber.ValidateAndGetResult(_validator);
            if (!validationResult.IsSuccess)
            {
                return BadRequest(validationResult);
            }

            var result = await _mediator.Send(new AddAccountNumberCommand(AccountNumber));
            
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

    [HttpPut]
    [ProducesResponseType(typeof(ResultDTO<AccountNumberDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> UpdateAccountNumber([FromBody] AccountNumberDTO AccountNumber)
    {
        try
        {
            // Fluent Validation
            var validationResult = AccountNumber.ValidateAndGetResult(_validator);
            if (!validationResult.IsSuccess)
            {
                return BadRequest(validationResult);
            }

            var command = new UpdateAccountNumberCommand(AccountNumber);
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
    public virtual async Task<IActionResult> DeleteAccountNumber(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteAccountNumberCommand(id));
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
    [HttpGet("view")]
    [ProducesResponseType(typeof(ResultDTO<List<AccountNumberViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetAccountNumberViewAll()
    {
        try
        {
            var result = await _mediator.Send(new GetAccountNumberViewAllQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/paging/{pageNumber:int:min(1)}/{pageSize:int:min(1)}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<AccountNumberViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetAccountNumberViewPaging(int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetAccountNumberViewAllPagingQuery(pageNumber, pageSize));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/{id:guid}")]
    [ProducesResponseType(typeof(ResultDTO<AccountNumberViewDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetAccountNumberViewById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetAccountNumberViewByIdQuery(id));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("view/OrganizationalChart/{organizationalChartId:guid}")]
    [ProducesResponseType(typeof(ResultDTO<List<AccountNumberViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetAccountNumberViewsByOrganizationalChartId(Guid organizationalChartId)
    {
        try
        {
            var result = await _mediator.Send(new GetAccountNumberViewsByOrganizationalChartIdQuery(organizationalChartId));
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

    [HttpGet("view/OrganizationalChart/{organizationalChartId:guid}/Paging/{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<AccountNumberViewDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetAccountNumberViewsByOrganizationalChartIdPaging(Guid organizationalChartId, int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetAccountNumberViewsByOrganizationalChartIdPagingQuery(organizationalChartId, pageNumber, pageSize));
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
