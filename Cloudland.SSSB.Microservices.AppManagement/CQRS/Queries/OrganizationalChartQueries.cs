using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetOrganizationalChartByIdQuery(Guid Id) : BaseQueryById<OrganizationalChartDTO, Guid>(Id);
public sealed record GetOrganizationalChartPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<OrganizationalChartDTO>(PageNumber, PageSize);
