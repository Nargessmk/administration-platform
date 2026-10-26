using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddLocationCommand(LocationDTO Location) : AddCommand<LocationDTO>(Location);
public sealed record AddLocationWithConstantCommand(LocationDTO Location, Guid? ParentId) : AddCommand<LocationDTO>(Location);
public sealed record UpdateLocationCommand(LocationDTO Location) : UpdateCommand<LocationDTO>(Location);
public sealed record DeleteLocationCommand(Guid Id) : DeleteCommand<Guid>(Id);
public sealed record SetLocationAdminCommand(Guid LocationId, Guid AdminId) : MediatR.IRequest<ResultDTO<bool>>;
