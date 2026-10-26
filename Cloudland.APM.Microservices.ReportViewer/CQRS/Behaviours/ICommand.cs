using MediatR;

namespace Cloudland.APM.Microservices.ReportViewer.CQRS.Behaviours;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}