using Cloudland.APM.DTO;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

// View queries
public record GetVehicleViewAllQuery
    : IRequest<ResultDTO<List<VehicleViewDTO>>>;

public record GetVehicleViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<VehicleViewDTO>>>;

public record GetVehicleViewByIdQuery(Guid Id) 
    : IRequest<ResultDTO<VehicleViewDTO>>;

public record GetVehicleViewChildrenByParentIdPagingQuery(Guid ParentId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<VehicleViewDTO>>>;

// Recursive queries
public record GetVehicleRecursiveAllQuery(Guid RootId) 
    : IRequest<ResultDTO<List<ConstantRecursiveDTO>>>;

public record GetVehicleRecursivePagingQuery(Guid RootId, int PageNumber, int PageSize) 
    : IRequest<ResultDTO<PaginatedResult<ConstantRecursiveDTO>>>;

public record GetVehicleRecursiveByIdQuery(Guid RootId, Guid ChildId) 
    : IRequest<ResultDTO<ConstantRecursiveDTO>>;
