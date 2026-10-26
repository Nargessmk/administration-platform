using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddContractCommand(ContractDTO Contract) : AddCommand<ContractDTO>(Contract);
public sealed record UpdateContractCommand(ContractDTO Contract) : UpdateCommand<ContractDTO>(Contract);
public sealed record DeleteContractCommand(Guid Id) : DeleteCommand<Guid>(Id);


