using MediatR;

namespace Cloudland.APM.BackgroundTask.CQRS.Behaviours;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling {Name}", typeof(TRequest).Name);

        var response = await next();

        logger.LogInformation("Handled {Name}", typeof(TResponse).Name);

        return response;
    }
}