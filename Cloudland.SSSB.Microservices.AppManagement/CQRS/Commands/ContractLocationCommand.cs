using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddContractLocationCommand(ContractLocationDTO ContractLocation) : AddCommand<ContractLocationDTO>(ContractLocation);
public sealed record UpdateContractLocationCommand(ContractLocationDTO ContractLocation) : UpdateCommand<ContractLocationDTO>(ContractLocation);
public sealed record DeleteContractLocationCommand(Guid Id) : DeleteCommand<Guid>(Id);


