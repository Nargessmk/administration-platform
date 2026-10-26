using MediatR;

namespace Cloudland.APM.Microservices.AppPermissionManagement.CQRS.Behaviours;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}