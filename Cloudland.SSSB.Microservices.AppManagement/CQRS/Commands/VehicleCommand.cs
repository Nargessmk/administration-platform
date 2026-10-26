using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddVehicleCommand(VehicleDTO Vehicle) : AddCommand<VehicleDTO>(Vehicle);
public sealed record AddVehicleWithConstantCommand(VehicleDTO Vehicle, Guid? ParentId) : AddCommand<VehicleDTO>(Vehicle);
public sealed record UpdateVehicleCommand(VehicleDTO Vehicle) : UpdateCommand<VehicleDTO>(Vehicle);
public sealed record DeleteVehicleCommand(Guid Id) : DeleteCommand<Guid>(Id);
