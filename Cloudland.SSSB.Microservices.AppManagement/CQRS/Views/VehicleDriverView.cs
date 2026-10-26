using Cloudland.APM.DTO;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

// View queries
public record GetVehicleDriverViewAllQuery
    : IRequest<ResultDTO<List<VehicleDriverViewDTO>>>;

public record GetVehicleDriverViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<VehicleDriverViewDTO>>>;

public record GetVehicleDriverViewByIdQuery(Guid Id) 
    : IRequest<ResultDTO<VehicleDriverViewDTO>>;

public record GetVehicleDriverViewChildrenByParentIdPagingQuery(Guid ParentId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<VehicleDriverViewDTO>>>;

public record GetVehicleDriverViewsByVehicleIdQuery(Guid VehicleId)
    : IRequest<ResultDTO<List<VehicleDriverViewDTO>>>;

public record GetVehicleDriverViewsByVehicleIdPagingQuery(Guid VehicleId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<VehicleDriverViewDTO>>>;

public record GetVehicleDriverViewsByDepartmentIdQuery(Guid DepartmentId)
    : IRequest<ResultDTO<List<VehicleDriverViewDTO>>>;

public record GetVehicleDriverViewsByDepartmentIdPagingQuery(Guid DepartmentId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<VehicleDriverViewDTO>>>;