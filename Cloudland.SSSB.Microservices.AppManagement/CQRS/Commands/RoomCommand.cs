using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddRoomCommand(RoomDTO Room) : AddCommand<RoomDTO>(Room);
public sealed record UpdateRoomCommand(RoomDTO Room) : UpdateCommand<RoomDTO>(Room);
public sealed record DeleteRoomCommand(Guid Id) : DeleteCommand<Guid>(Id);
