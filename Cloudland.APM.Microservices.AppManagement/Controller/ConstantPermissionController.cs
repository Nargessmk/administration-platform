using System.Net;
using Asp.Versioning;
using AutoMapper;
using MassTransit;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.Controller.Base;
using Cloudland.APM.Microservices.AppManagement.CQRS.Commands;
using Cloudland.APM.Microservices.AppManagement.CQRS.Queries;
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
public class ConstantPermissionController(
    IMediator mediator,
    IMapper mapper,
    IDistributedCache distributedCache,
    IElasticClient elasticClient,
    IPublishEndpoint publishEndpoint) :
    BaseController<ConstantDTO, Guid>(mediator, mapper, distributedCache, elasticClient, publishEndpoint)
{
    [HttpGet("GetConstantWithPermissionForRole/{rootId:guid}/{applicationId:guid}/{roleId:guid}")]
    [ProducesResponseType(typeof(ConstantRecursiveDTO), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetConstantWithPermissionForRole(Guid rootId, Guid applicationId, Guid? roleId)
    {
        var constant = await _mediator.Send(new GetConstantPermissionRecursive(
            rootId,
            applicationId,
            null,
            roleId,
            null));
        return Ok(constant);
    }

    [HttpGet("GetConstantWithPermissionForUser/{rootId:guid}/{applicationId:guid}/{userId:guid}")]
    [ProducesResponseType(typeof(ConstantRecursiveDTO), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetConstantWithPermissionForUser(Guid rootId, Guid applicationId, Guid? userId)
    {
        var constant = await _mediator.Send(new GetConstantPermissionRecursive(
            rootId,
            applicationId,
            userId,
            null,
            null));
        return Ok(constant);
    }

    [HttpGet("GetConstantWithPermissionForGroup/{rootId:guid}/{applicationId:guid}/{groupId:guid}")]
    [ProducesResponseType(typeof(ConstantRecursiveDTO), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetConstantWithPermissionForGroup(Guid rootId, Guid applicationId, Guid? groupId)
    {
        var constant = await _mediator.Send(new GetConstantPermissionRecursive(
            rootId,
            applicationId,
            null,
            null,
            groupId));
        return Ok(constant);
    }


    [HttpPost("UpdateConstantPermission")]
    [ProducesResponseType(typeof(List<UserConstantPermissionDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(int), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> UpdateConstantPermission([FromQuery] Guid? userId, Guid? roleId, Guid? groupId,
        [FromBody] Guid[] constants)
    {
        var updatePermissions =
            await _mediator.Send(new UpdateConstantPermissionCommand(userId, roleId, groupId, constants));
        return Ok(updatePermissions);
    }
}