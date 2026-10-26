using Cloudland.APM.DTO;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

// View queries
public record GetWeekDayViewAllQuery
    : IRequest<ResultDTO<List<WeekDayViewDTO>>>;

public record GetWeekDayViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<WeekDayViewDTO>>>;

public record GetWeekDayViewByIdQuery(Guid Id) 
    : IRequest<ResultDTO<WeekDayViewDTO>>;

public record GetWeekDayViewChildrenByParentIdPagingQuery(Guid ParentId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<WeekDayViewDTO>>>;

// Recursive queries
public record GetWeekDayRecursiveAllQuery(Guid RootId) 
    : IRequest<ResultDTO<List<ConstantRecursiveDTO>>>;

public record GetWeekDayRecursivePagingQuery(Guid RootId, int PageNumber, int PageSize) 
    : IRequest<ResultDTO<PaginatedResult<ConstantRecursiveDTO>>>;

public record GetWeekDayRecursiveByIdQuery(Guid RootId, Guid ChildId) 
    : IRequest<ResultDTO<ConstantRecursiveDTO>>;


