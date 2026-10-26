using Asp.Versioning;
using AutoMapper;
using MassTransit;
using Cloudland.APM.DTO;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Nest;

namespace Cloudland.APM.Microservices.AppManagement.Controller.Base;

[ApiVersion("1.0")]
[Route("api/v1/[controller]")]
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Authorize]
public class BaseController<T, TV>(
    IMediator mediator,
    IMapper mapper,
    IDistributedCache distributedCache,
    IElasticClient elasticClient,
    IPublishEndpoint publishEndpoint)
    : ControllerBase
    where T : BaseDTO<TV>
{
    protected readonly IMediator _mediator = mediator;
    protected readonly IMapper _mapper = mapper;
    protected readonly IDistributedCache _distributedCache = distributedCache;
    protected readonly IElasticClient _elasticClient = elasticClient;
    protected readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
}