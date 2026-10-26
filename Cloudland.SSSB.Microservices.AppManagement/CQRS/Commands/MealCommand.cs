using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddMealCommand(MealDTO Meal) : AddCommand<MealDTO>(Meal);
public sealed record AddMealWithConstantCommand(MealDTO Meal, Guid? ParentId) : AddCommand<MealDTO>(Meal);
public sealed record UpdateMealCommand(MealDTO Meal) : UpdateCommand<MealDTO>(Meal);
public sealed record DeleteMealCommand(Guid Id) : DeleteCommand<Guid>(Id);
