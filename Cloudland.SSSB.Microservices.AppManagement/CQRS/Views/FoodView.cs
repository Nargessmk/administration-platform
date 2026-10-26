using Cloudland.APM.DTO;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

// View queries
public record GetFoodViewAllQuery
    : IRequest<ResultDTO<List<FoodViewDTO>>>;

public record GetFoodViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<FoodViewDTO>>>;

public record GetFoodViewByIdQuery(Guid Id) 
    : IRequest<ResultDTO<FoodViewDTO>>;

public record GetFoodViewChildrenByParentIdPagingQuery(Guid ParentId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<FoodViewDTO>>>;

// Recursive queries
public record GetFoodRecursiveAllQuery(Guid RootId) 
    : IRequest<ResultDTO<List<ConstantRecursiveDTO>>>;

public record GetFoodRecursivePagingQuery(Guid RootId, int PageNumber, int PageSize) 
    : IRequest<ResultDTO<PaginatedResult<ConstantRecursiveDTO>>>;

public record GetFoodRecursiveByIdQuery(Guid RootId, Guid ChildId) 
    : IRequest<ResultDTO<ConstantRecursiveDTO>>;

public record GetFoodViewsByMealIdQuery(Guid MealId)
    : IRequest<ResultDTO<List<FoodViewDTO>>>;

public record GetFoodViewsByMealIdPagingQuery(Guid MealId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<FoodViewDTO>>>;
