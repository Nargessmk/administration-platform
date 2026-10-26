using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddVehicleDriverCommand(VehicleDriverDTO VehicleDriver) : AddCommand<VehicleDriverDTO>(VehicleDriver);
public sealed record UpdateVehicleDriverCommand(VehicleDriverDTO VehicleDriver) : UpdateCommand<VehicleDriverDTO>(VehicleDriver);
public sealed record DeleteVehicleDriverCommand(Guid Id) : DeleteCommand<Guid>(Id);
