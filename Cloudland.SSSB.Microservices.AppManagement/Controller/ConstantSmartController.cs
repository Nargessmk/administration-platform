using System.Net;
using Asp.Versioning;
using AutoMapper;
using Cloudland.APM.DTO;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
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
public class ConstantSmartController(
	IMediator mediator,
	IMapper mapper,
	IDistributedCache distributedCache,
	IElasticClient elasticClient,
	IPublishEndpoint publishEndpoint) :
	BaseController<BaseDTO<Guid>, Guid>(mediator, mapper, distributedCache, elasticClient, publishEndpoint)
{
	[HttpDelete("{id:guid}")]
	[ProducesResponseType(typeof(ResultDTO<int>), (int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.BadRequest)]
	[ProducesResponseType((int)HttpStatusCode.Unauthorized)]
	public virtual async Task<IActionResult> DeleteConstantSmart(Guid id)
	{
		try
		{
			var result = await _mediator.Send(new DeleteConstantSmartCommand(id));
			if (result != null && result.IsSuccess)
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
