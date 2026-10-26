using AutoMapper;
using Cloudland.APM.Models;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;

public abstract class BaseHandler<T, TKey>(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<T> repository,
    IMapper map,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor, // پارامتر ورودی
    IAsyncDocumentSession dbSession)
    where T : BaseModel<TKey>
{
    protected readonly IUnitOfWork _unitOfWork = unitOfWork;
    protected readonly EntityFrameworkCore.Repository.Interfaces.IRepository<T> _repository = repository;
    protected readonly IMapper _mapper = map;
    protected readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    protected readonly IElasticClient _elasticClient = elasticClient;
    protected readonly IDistributedCache _distributedCache = distributedCache;
    protected readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor; 
    protected readonly IAsyncDocumentSession _dbSession = dbSession;
}