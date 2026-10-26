using System.Net;
using Asp.Versioning;
using AutoMapper;
using MassTransit;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.Controller.Base;
using Cloudland.APM.Microservices.AppManagement.CQRS.Commands;
using Cloudland.APM.Microservices.AppManagement.CQRS.Notifications;
using Cloudland.APM.Microservices.AppManagement.CQRS.Queries;
using Cloudland.APM.Models.ResultModel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Sin.Microservices.Course.CustomerFilterActions;

namespace Cloudland.APM.Microservices.AppManagement.Controller;

[ApiVersion("1.0")]
[Route("api/v1/[controller]")]
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Authorize]
public class ConstantController(
    IMediator mediator,
    IMapper mapper,
    IDistributedCache distributedCache,
    IElasticClient elasticClient,
    IPublishEndpoint publishEndpoint) :
    BaseController<ConstantDTO, Guid>(mediator, mapper, distributedCache, elasticClient, publishEndpoint)
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ConstantDTO>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetRootConstant()
    {
        var constant = await _mediator.Send(new GetRootConstantQuery());
        return Ok(constant);
    }


    [HttpGet("ById/{id:guid}")]
    [ProducesResponseType(typeof(ConstantDTO), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetConstantById(Guid id)
    {
        var constant = await _mediator.Send(new GetConstantByIdQuery(id));
        return Ok(constant);
    }
    
    [HttpGet("ByIds")]
    [ProducesResponseType(typeof(List<ConstantDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult> GetConstantsByIds([FromBody] List<Guid> ids)
    {
        if (ids == null || !ids.Any())
        {
            return BadRequest("IDs list cannot be null or empty");
        }

        var constants = await _mediator.Send(new GetConstantsByIdsQuery(ids));
        return Ok(constants);
    }

    [HttpGet("ChildsByRootId/{rootId:guid}")]
    [ProducesResponseType(typeof(List<ConstantDTO>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetRootChilds(Guid rootId)
    {
        var constant = await _mediator.Send(new GetRootChildConstantQuery(rootId));
        return Ok(constant);
    }
    
    [HttpGet("ByDate/{rootId:guid}/{dateTime:datetime}")]
    [ProducesResponseType(typeof(ConstantRecursiveDTO), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetConstantById(Guid rootId, DateTime dateTime)
    {
        var constant = await _mediator.Send(new GetConstantByDateQuery(rootId, dateTime));
        return Ok(constant);
    }


    [HttpGet("ByToday/{rootId:guid}")]
    [ProducesResponseType(typeof(ConstantDTO), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetByToday(Guid rootId)
    {
        var constant = await _mediator.Send(new GetConstantByDateQuery(rootId, DateTime.Now));
        return Ok(constant);
    }

    [HttpGet("TreeViewByRootId/{rootId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ConstantDTO>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetTreeConstant(Guid rootId)
    {
        var constant = await _mediator.Send(new GetTreeConstantQuery(rootId));
        return Ok(constant);
    }

    [HttpGet("PagingChildsByRootId/{rootId:guid}/{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType(typeof(PagedResult<ConstantDTO>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetRootConstantPaging(Guid rootId, int pageNumber, int pageSize)
    {
        var constant = await _mediator.Send(new GetRootConstantPagingQuery(rootId, pageNumber, pageSize));
        return Ok(constant);
    }

    [HttpGet("GetConstantRecursiveByRootId/{rootId:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ConstantRecursiveDTO), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetConstantRecursiveByRootId(Guid rootId)
    {
        var course = await _mediator.Send(new GetConstantRecursiveByRootId(rootId));
        return Ok(course);
    }

    [HttpGet("GetConstantNodeType/{nodeId:guid}/{nodeTypeId}")]
    [ProducesResponseType(typeof(IEnumerable<ConstantDTO>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetConstantNodeType(Guid nodeId, Guid? nodeTypeId)
    {
        var course = await _mediator.Send(new GetConstantNodeTypeByIdQuery(nodeId, nodeTypeId));
        return Ok(course);
    }


    [HttpPut("StoreJsonData/{nodeId:guid}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> StoreJsonData(
        [FromRoute] Guid nodeId,
        [FromQuery] string? nodeType,
        [FromBody] string dataObject) // Accept any JSON
    {
        var result = await _mediator.Send(new StoreJsonDataCommand(nodeId, nodeType, dataObject));
        return Ok(result);
    }

    [HttpGet("RetrieveJsonData/{nodeId:guid}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> RetrieveJsonData(
        [FromRoute] Guid nodeId,[FromQuery] string? nodeType)  
    {
        var result = await _mediator.Send(new RetrieveJsonDataQuery(nodeId,nodeType));
        return Ok(result);
    }

    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ConstantDTO), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ModelValidation]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AddConstant([FromBody] ConstantDTO constant)
    {
        var constantToReturn = await _mediator.Send(new AddConstantCommand(constant));
        if (constantToReturn == null) return BadRequest();
        await _mediator.Publish(new ConstantAddedNotification(constantToReturn));
        return Ok(constantToReturn);
    }

    [HttpPut]
    [Produces("application/json")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ModelValidation]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateConstant([FromBody] ConstantDTO constant)
    {
        var constantToReturn = await _mediator.Send(new UpdateConstantCommand(constant));

        await _mediator.Publish(new ConstantUpdatedNotification(constantToReturn));
        return Ok(constantToReturn);
    }

    [HttpDelete("{id:guid}")]
    [Produces("application/json")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteConstant(Guid id)
    {
        var constant = await _mediator.Send(new GetConstantByIdQuery(id));
        try
        {
            await _mediator.Send(new DeleteConstantCommand(id));
            await _mediator.Publish(new ConstantDeletedNotification(constant));
            return NoContent();
        }
        catch
        {
            return BadRequest("Node has a child");
        }
    }

    [HttpDelete("DeleteAllChild/{parentId:guid}")]
    [Produces("application/json")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteAllChildByParentIdConstant(Guid parentId)
    {
        var constant = await _mediator.Send(new GetTreeConstantQuery(parentId));
        var result = await _mediator.Send(new DeleteAllChildsConstantCommandByParentId(parentId));
        if (result == 0)
            return BadRequest("Node has a child");
        await _mediator.Publish(new ConstantDeletedGroupNotification(constant));
        return NoContent();
    }
}