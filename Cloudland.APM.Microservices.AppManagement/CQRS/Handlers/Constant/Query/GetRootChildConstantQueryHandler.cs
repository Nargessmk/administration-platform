using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.APM.Microservices.AppManagement.CQRS.Queries;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Constant.Query;

public sealed class GetRootChildConstantQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Cloudland.APM.Models.Constant> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IConfiguration configuration,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<GetRootChildConstantQueryHandler> logger)
    : BaseHandler<Cloudland.APM.Models.Constant, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetRootChildConstantQuery, List<ConstantDTO>>
{
    public async Task<List<ConstantDTO>> Handle(GetRootChildConstantQuery request,
        CancellationToken cancellationToken)
    {
        
        var cacheSection = configuration.GetSection("ConstantsCache");
        var allowedRootIds = cacheSection.GetSection("AllowedRootIds").Get<List<Guid>>() ?? new List<Guid>();
        var expirationMinutes = cacheSection.GetValue("ExpirationMinutes", 30);

        
        var shouldUseCache = allowedRootIds.Contains(request.RootId);
        
        logger.LogDebug("Processing GetRootChildConstantQuery for RootId {RootId}, Caching enabled: {ShouldUseCache}", 
            request.RootId, shouldUseCache);

        if (shouldUseCache)
        {
            var cacheKey = $"Constants_RootId_{request.RootId}";

            
            var cachedData = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedData))
            {
                try
                {
                    var cachedItems = System.Text.Json.JsonSerializer.Deserialize<List<ConstantDTO>>(cachedData);
                    if (cachedItems != null)
                    {
                        logger.LogDebug("Retrieved {Count} constants from cache for RootId {RootId}", 
                            cachedItems.Count, request.RootId);
                        return cachedItems;
                    }
                }
                catch (System.Text.Json.JsonException ex)
                {
                    
                    logger.LogWarning(ex, "Failed to deserialize cached data for RootId {RootId}, removing corrupted cache entry", request.RootId);
                    await _distributedCache.RemoveAsync(cacheKey, cancellationToken);
                }
            }
        }

        
        var query = _repository.MultipleResultQuery()
            .AndFilter(x => x.ParentId == request.RootId)
            .OrderBy(x => x.Priority);
            
        var items = await _repository.SearchAsync(query, cancellationToken);
        var result = _mapper.Map<List<ConstantDTO>>(items);

        
        if (shouldUseCache)
        {
            try
            {
                var serializedData = System.Text.Json.JsonSerializer.Serialize(result);
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes)
                };
                await _distributedCache.SetStringAsync($"Constants_RootId_{request.RootId}", serializedData, cacheOptions, cancellationToken);
                logger.LogDebug("Cached {Count} constants for RootId {RootId} with expiration of {ExpirationMinutes} minutes", 
                    result.Count, request.RootId, expirationMinutes);
            }
            catch (Exception ex)
            {
                
                logger.LogError(ex, "Failed to cache data for RootId {RootId}", request.RootId);
            }
        }

        return result;
    }
}