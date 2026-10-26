using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetVehicleByIdQuery(Guid Id) : BaseQueryById<VehicleDTO, Guid>(Id);
public sealed record GetVehiclePagingQuery(int PageNumber, int PageSize)
    : BaseQueryPaging<VehicleDTO>(PageNumber, PageSize);

public sealed record GetDepartmentVehiclesQuery(Guid DepartmentId) : BaseQueryListById<VehicleDTO, Guid>(DepartmentId);

public sealed record GetDepartmentVehiclesPagingQuery(Guid DepartmentId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<VehicleDTO>>>;

public sealed record GetVehicleTypeVehiclesQuery(Guid VehicleTypeId) : BaseQueryListById<VehicleDTO, Guid>(VehicleTypeId);

public sealed record GetVehicleTypeVehiclesPagingQuery(Guid VehicleTypeId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<VehicleDTO>>>;
