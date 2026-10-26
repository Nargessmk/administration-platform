using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Behaviours;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}