using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

// View queries
public record GetContractFoodCalendarViewAllQuery
    : IRequest<ResultDTO<List<ContractFoodCalendarViewDTO>>>;

public record GetContractFoodCalendarViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>>;

public record GetContractFoodCalendarViewByIdQuery(Guid Id) 
    : IRequest<ResultDTO<ContractFoodCalendarViewDTO>>;

public record GetContractFoodCalendarViewByContractFoodLocationIdQuery(Guid ContractFoodLocationId) 
    : IRequest<ResultDTO<List<ContractFoodCalendarViewDTO>>>;

public record GetContractFoodCalendarViewByContractFoodLocationIdPagingQuery(Guid ContractFoodLocationId, int PageNumber, int PageSize) 
    : IRequest<ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>>;

public record GetContractFoodCalendarViewByLocationIdQuery(Guid LocationId) 
    : IRequest<ResultDTO<List<ContractFoodCalendarViewDTO>>>;

public record GetContractFoodCalendarViewByLocationIdPagingQuery(Guid LocationId, int PageNumber, int PageSize) 
    : IRequest<ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>>;

public record GetContractFoodCalendarViewByLocationIdAndContractIdQuery(Guid LocationId, Guid ContractId)
    : IRequest<ResultDTO<List<ContractFoodCalendarViewDTO>>>;

public record GetContractFoodCalendarViewByLocationIdAndContractIdPagingQuery(Guid LocationId, Guid ContractId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>>;

public record GetContractFoodCalendarViewByLocationIdAndContractIdAndMonthQuery(Guid LocationId, Guid ContractId, string Month)
    : IRequest<ResultDTO<List<ContractFoodCalendarViewDTO>>>;

public record GetContractFoodCalendarViewByLocationIdAndContractIdAndMonthPagingQuery(Guid LocationId, Guid ContractId, string Month, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>>;

public record GetContractFoodCalendarViewByLocationIdAndMonthQuery(Guid LocationId, string Month)
    : IRequest<ResultDTO<List<ContractFoodCalendarViewDTO>>>;

public record GetContractFoodCalendarViewByLocationIdAndMonthPagingQuery(Guid LocationId, string Month, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>>;

public record GetContractFoodCalendarViewByLocationIdAndContractIdAndMonthAndMealIdQuery(Guid LocationId, Guid ContractId, string Month, Guid MealId)
    : IRequest<ResultDTO<List<ContractFoodCalendarViewDTO>>>;

public record GetContractFoodCalendarViewByLocationIdAndContractIdAndMonthAndMealIdPagingQuery(Guid LocationId, Guid ContractId, string Month, Guid MealId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>>;

