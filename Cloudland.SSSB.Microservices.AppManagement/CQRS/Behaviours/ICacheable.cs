namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Behaviours;

public interface ICacheable
{
    bool BypassCache { get; }
    string CacheKey { get; }
    int SlidingExpirationInMinutes { get; }
    int AbsoluteExpirationInMinutes { get; }
}