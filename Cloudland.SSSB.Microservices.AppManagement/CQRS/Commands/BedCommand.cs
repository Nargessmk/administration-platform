using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddBedCommand(BedDTO Bed) : AddCommand<BedDTO>(Bed);
public sealed record UpdateBedCommand(BedDTO Bed) : UpdateCommand<BedDTO>(Bed);
public sealed record DeleteBedCommand(Guid Id) : DeleteCommand<Guid>(Id);
