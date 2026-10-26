using System.Collections.Concurrent;
using System.Net;

namespace Cloudland.APM.Microservices.Gateway.Extensions;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, RequestCounter> _counters = new();

    private static readonly Dictionary<string, (int Limit, int WindowSeconds)> EndpointLimits = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/ReservationFoodCalendar/"] = (40, 60),
        ["/ReservationRequestHeader/"] = (40, 60),
        ["/ReservationRequestPerson/"] = (40, 60),
        ["/ReservationRequestBed/"] = (40, 60),
        ["/ReservationRequestLetter/"] = (40, 60),
        ["/ContractFoodCalendar/"] = (40, 60),
    };

    private const int GlobalLimit = 100;
    private const int GlobalWindowSeconds = 60;
    private const int LocalhostGlobalLimit = 5000;

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    private static bool IsLocalOrLoopback(HttpContext context, string ip)
    {
        if (string.Equals(ip, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(ip, "::1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(ip, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (context.Connection.RemoteIpAddress != null && IPAddress.IsLoopback(context.Connection.RemoteIpAddress))
        {
            return true;
        }

        return false;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.Request.Path.Value ?? "";
        var now = DateTime.UtcNow;
        var isLocal = IsLocalOrLoopback(context, ip);

        // Find matching endpoint limit
        var matchedPrefix = EndpointLimits.Keys.FirstOrDefault(prefix =>
            path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        if (matchedPrefix != null)
        {
            var (limit, window) = EndpointLimits[matchedPrefix];
            if (isLocal)
            {
                limit *= 5; // Relax endpoint limits for local development
            }
            var key = $"ep_{ip}_{matchedPrefix}";

            if (!TryIncrementAndCheck(key, limit, window, now))
            {
                context.Response.StatusCode = 429;
                context.Response.Headers.RetryAfter = window.ToString();
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Rate limit exceeded",
                    message = $"Too many requests. Limit: {limit} per {window} seconds",
                    retryAfter = window
                });
                return;
            }
        }

        // Global limit
        var effectiveGlobalLimit = isLocal ? LocalhostGlobalLimit : GlobalLimit;
        var globalKey = $"global_{ip}";
        if (!TryIncrementAndCheck(globalKey, effectiveGlobalLimit, GlobalWindowSeconds, now))
        {
            context.Response.StatusCode = 429;
            context.Response.Headers.RetryAfter = GlobalWindowSeconds.ToString();
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                message = $"Too many requests. Limit: {effectiveGlobalLimit} per {GlobalWindowSeconds} seconds",
                retryAfter = GlobalWindowSeconds
            });
            return;
        }

        await _next(context);
    }

    private static bool TryIncrementAndCheck(string key, int limit, int windowSeconds, DateTime now)
    {
        var counter = _counters.GetOrAdd(key, _ => new RequestCounter());

        lock (counter)
        {
            if (now.Subtract(counter.WindowStart).TotalSeconds >= windowSeconds)
            {
                counter.WindowStart = now;
                counter.Count = 0;
            }

            counter.Count++;

            return counter.Count <= limit;
        }
    }
}

public class RequestCounter
{
    public DateTime WindowStart { get; set; } = DateTime.UtcNow;
    public int Count { get; set; }
}
