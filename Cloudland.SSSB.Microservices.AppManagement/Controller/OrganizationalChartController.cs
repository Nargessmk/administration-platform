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
public class OrganizationalChartController(
    IMediator mediator,
    IMapper mapper,
    IDistributedCache distributedCache,
    IElasticClient elasticClient,
    IPublishEndpoint publishEndpoint) :
    BaseController<OrganizationalChartDTO, Guid>(mediator, mapper, distributedCache, elasticClient, publishEndpoint)
{
    private readonly OrganizationalChartDTOValidator _validator = new();

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResultDTO<OrganizationalChartDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetOrganizationalChartById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetOrganizationalChartByIdQuery(id));
            if (result is { IsSuccess: true })
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
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<OrganizationalChartDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetPagingOrganizationalCharts(int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetOrganizationalChartPagingQuery(pageNumber, pageSize));
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
    [ProducesResponseType(typeof(ResultDTO<OrganizationalChartDTO>), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> CreateOrganizationalChart([FromBody] OrganizationalChartDTO organizationalChart)
    {
        
        try
        {
            // Fluent Validation
            var validationResult = organizationalChart.ValidateAndGetResult(_validator);
            if (!validationResult.IsSuccess)
            {
                return BadRequest(validationResult);
            }

            var result = await _mediator.Send(new AddOrganizationalChartCommand(organizationalChart));
            
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

    [HttpPost("{parentId:guid}")]
    [ProducesResponseType(typeof(ResultDTO<OrganizationalChartDTO>), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> CreateOrganizationalChartWithParent([FromRoute] Guid parentId, [FromBody] OrganizationalChartDTO organizationalChart)
    {
        try
        {
            var validationResult = organizationalChart.ValidateAndGetResult(_validator);
            if (!validationResult.IsSuccess)
            {
                return BadRequest(validationResult);
            }

            var result = await _mediator.Send(new AddOrganizationalChartWithConstantCommand(organizationalChart, parentId));
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
    [ProducesResponseType(typeof(ResultDTO<OrganizationalChartDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> UpdateOrganizationalChart([FromBody] OrganizationalChartDTO organizationalChart)
    {
        try
        {
            // Fluent Validation
            var validationResult = organizationalChart.ValidateAndGetResult(_validator);
            if (!validationResult.IsSuccess)
            {
                return BadRequest(validationResult);
            }

            var command = new UpdateOrganizationalChartCommand(organizationalChart);
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
    public virtual async Task<IActionResult> DeleteOrganizationalChart(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteOrganizationalChartCommand(id));
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

    #region Views and Recursives
    
    [HttpGet("view/all")]
    [ProducesResponseType(typeof(ResultDTO<List<OrganizationalChartView>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public virtual async Task<IActionResult> GetOrganizationalChartViewAll()
    {
        try
        {
            var result = await _mediator.Send(new GetOrganizationalChartViewAllQuery());
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpGet("View/Paging/{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<OrganizationalChartView>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public virtual async Task<IActionResult> GetOrganizationalChartViewAllPaging(int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetOrganizationalChartViewAllPagingQuery(pageNumber, pageSize));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpGet("View/{id:guid}")]
    [ProducesResponseType(typeof(ResultDTO<OrganizationalChartView>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetOrganizationalChartViewById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetOrganizationalChartViewByIdQuery(id));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
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

    [HttpGet("View/Children/{parentId:guid}/Paging/{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<OrganizationalChartView>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public virtual async Task<IActionResult> GetOrganizationalChartViewChildrenByParentIdPaging(Guid parentId, int pageNumber, int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetOrganizationalChartViewChildrenByParentIdPagingQuery(parentId, pageNumber, pageSize));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("Recursive/All/{rootId:guid}")]
    [ProducesResponseType(typeof(ResultDTO<List<OrganizationalChartDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public virtual async Task<IActionResult> GetOrganizationalChartRecursiveAll(Guid rootId)
    {
        try
        {
            var result = await _mediator.Send(new GetOrganizationalChartRecursiveAllQuery(rootId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("Recursive/Paging/{rootId:guid}")]
    [ProducesResponseType(typeof(ResultDTO<PaginatedResult<OrganizationalChartDTO>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public virtual async Task<IActionResult> GetOrganizationalChartRecursivePaging(Guid rootId, [FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        try
        {
            var result = await _mediator.Send(new GetOrganizationalChartRecursivePagingQuery(rootId, pageNumber, pageSize));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("Recursive/{rootId:guid}/{childId:guid}")]
    [ProducesResponseType(typeof(ResultDTO<OrganizationalChartDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public virtual async Task<IActionResult> GetOrganizationalChartRecursiveById(Guid rootId, Guid childId)
    {
        try
        {
            var result = await _mediator.Send(new GetOrganizationalChartRecursiveByIdQuery(rootId, childId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
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

    #endregion
    
}
