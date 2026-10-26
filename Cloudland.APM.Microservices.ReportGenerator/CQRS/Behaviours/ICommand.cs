using MediatR;

namespace Cloudland.APM.Microservices.ReportGenerator.CQRS.Behaviours;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}