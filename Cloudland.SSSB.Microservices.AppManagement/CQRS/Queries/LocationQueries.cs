using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetLocationByIdQuery(Guid Id) : BaseQueryById<LocationDTO, Guid>(Id);
public sealed record GetLocationPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<LocationDTO>(PageNumber, PageSize);
