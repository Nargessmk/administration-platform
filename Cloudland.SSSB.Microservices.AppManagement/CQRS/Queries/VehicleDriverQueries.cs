using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetVehicleDriverByIdQuery(Guid Id) : BaseQueryById<VehicleDriverDTO, Guid>(Id);
public sealed record GetVehicleDriverPagingQuery(int PageNumber, int PageSize)
    : BaseQueryPaging<VehicleDriverDTO>(PageNumber, PageSize);

    