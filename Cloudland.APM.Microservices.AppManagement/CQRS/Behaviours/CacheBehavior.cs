using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Behaviours;

public sealed class CachingBehavior<TRequest, TResponse>(
    ILogger<CachingBehavior<TRequest, TResponse>> logger,
    IDistributedCache cache
)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheable
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        TResponse response;
        if (request.BypassCache) return await next();
        var cachedResponse = await cache.GetAsync(request.CacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            response = JsonSerializer.Deserialize<TResponse>(Encoding.UTF8.GetString(cachedResponse))!;
            logger.LogInformation("fetched from cache with key : {CacheKey}", request.CacheKey);
            await cache.RefreshAsync(request.CacheKey, cancellationToken);
        }
        else
        {
            response = await GetResponseAndAddToCache();
            logger.LogInformation("added to cache with key : {CacheKey}", request.CacheKey);
        }

        return response;

        async Task<TResponse> GetResponseAndAddToCache()
        {
            response = await next();
            if (response == null) return response;
            var slidingExpiration = request.SlidingExpirationInMinutes == 0 ? 30 : request.SlidingExpirationInMinutes;
            var absoluteExpiration =
                request.AbsoluteExpirationInMinutes == 0 ? 60 : request.AbsoluteExpirationInMinutes;
            var options = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(slidingExpiration))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(absoluteExpiration));

            var serializedData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
            await cache.SetAsync(request.CacheKey, serializedData, options, cancellationToken);
            return response;
        }
    }
}