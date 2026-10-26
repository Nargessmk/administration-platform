using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetOrganizationalPositionByIdQuery(Guid Id) : BaseQueryById<OrganizationalPositionDTO, Guid>(Id);
public sealed record GetOrganizationalPositionPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<OrganizationalPositionDTO>(PageNumber, PageSize);
