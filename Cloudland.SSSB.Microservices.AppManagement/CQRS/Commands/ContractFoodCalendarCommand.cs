using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddContractFoodCalendarCommand(ContractFoodCalendarDTO ContractFoodCalendar) : AddCommand<ContractFoodCalendarDTO>(ContractFoodCalendar);
public sealed record UpdateContractFoodCalendarCommand(ContractFoodCalendarDTO ContractFoodCalendar) : UpdateCommand<ContractFoodCalendarDTO>(ContractFoodCalendar);
public sealed record DeleteContractFoodCalendarCommand(Guid Id) : DeleteCommand<Guid>(Id);
public sealed record DeleteContractFoodCalendarByContractFoodLocationIdAndContractIdAndMealIdCommand(Guid ContractId, Guid FoodId, Guid LocationId, Guid MealId, string Date) : IRequest<ResultDTO<int>>;


