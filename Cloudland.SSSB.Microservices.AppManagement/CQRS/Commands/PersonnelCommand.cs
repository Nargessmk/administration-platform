using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddPersonnelCommand(PersonnelDTO Personnel) : AddCommand<PersonnelDTO>(Personnel);
public sealed record AddPersonnelWithConstantCommand(PersonnelDTO Personnel, Guid? ParentId) : AddCommand<PersonnelDTO>(Personnel);
public sealed record UpdatePersonnelCommand(PersonnelDTO Personnel) : UpdateCommand<PersonnelDTO>(Personnel);
public sealed record DeletePersonnelCommand(Guid Id) : DeleteCommand<Guid>(Id);
