using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddContractFoodLocationCommand(ContractFoodLocationDTO ContractFoodLocation) : AddCommand<ContractFoodLocationDTO>(ContractFoodLocation);
public sealed record UpdateContractFoodLocationCommand(ContractFoodLocationDTO ContractFoodLocation) : UpdateCommand<ContractFoodLocationDTO>(ContractFoodLocation);
public sealed record DeleteContractFoodLocationCommand(Guid Id) : DeleteCommand<Guid>(Id);
