using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddFoodCommand(FoodDTO Food) : AddCommand<FoodDTO>(Food);
public sealed record AddFoodWithConstantCommand(FoodDTO Food, Guid? ParentId) : AddCommand<FoodDTO>(Food);
public sealed record UpdateFoodCommand(FoodDTO Food) : UpdateCommand<FoodDTO>(Food);
public sealed record DeleteFoodCommand(Guid Id) : DeleteCommand<Guid>(Id);
