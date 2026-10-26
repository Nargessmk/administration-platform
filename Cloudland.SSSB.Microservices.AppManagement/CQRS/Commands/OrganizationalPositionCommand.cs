using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddOrganizationalPositionCommand(OrganizationalPositionDTO OrganizationalPosition) : AddCommand<OrganizationalPositionDTO>(OrganizationalPosition);
public sealed record AddOrganizationalPositionWithConstantCommand(OrganizationalPositionDTO OrganizationalPosition, Guid? ParentId) : AddCommand<OrganizationalPositionDTO>(OrganizationalPosition);
public sealed record UpdateOrganizationalPositionCommand(OrganizationalPositionDTO OrganizationalPosition) : UpdateCommand<OrganizationalPositionDTO>(OrganizationalPosition);
public sealed record DeleteOrganizationalPositionCommand(Guid Id) : DeleteCommand<Guid>(Id);
