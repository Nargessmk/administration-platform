using System.Diagnostics;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Behaviours;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = new();
        logger.LogInformation("Handling {Name}", typeof(TRequest).Name);
        stopwatch.Start();

        var response = await next();

        stopwatch.Stop();

        logger.LogInformation("Handled {Name} in {StopwatchElapsedMilliseconds} ms", typeof(TResponse).Name,
            stopwatch.ElapsedMilliseconds);
        stopwatch.Reset();

        return response;
    }
}