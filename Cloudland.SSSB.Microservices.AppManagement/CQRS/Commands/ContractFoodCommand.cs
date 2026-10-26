using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddContractFoodCommand(ContractFoodDTO ContractFood) : AddCommand<ContractFoodDTO>(ContractFood);
public sealed record UpdateContractFoodCommand(ContractFoodDTO ContractFood) : UpdateCommand<ContractFoodDTO>(ContractFood);
public sealed record DeleteContractFoodCommand(Guid Id) : DeleteCommand<Guid>(Id);


