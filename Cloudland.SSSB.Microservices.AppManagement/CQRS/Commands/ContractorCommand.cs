using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddContractorCommand(ContractorDTO Contractor) : AddCommand<ContractorDTO>(Contractor);
public sealed record AddContractorWithConstantCommand(ContractorDTO Contractor, Guid? ParentId) : AddCommand<ContractorDTO>(Contractor);
public sealed record UpdateContractorCommand(ContractorDTO Contractor) : UpdateCommand<ContractorDTO>(Contractor);
public sealed record DeleteContractorCommand(Guid Id) : DeleteCommand<Guid>(Id);
