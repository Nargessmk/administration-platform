using MediatR;
using Cloudland.SSSB.Microservices.AppManagement.Services;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public record ReindexByViewCommand(string ViewName) : IRequest<ReindexResult>;

public record ReindexAllCommand() : IRequest<ReindexResult>;

public record GetAvailableViewsCommand() : IRequest<List<string>>;
