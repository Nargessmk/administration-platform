using MediatR;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Behaviours;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}