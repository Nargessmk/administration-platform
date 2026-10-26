using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetPersonnelByIdQuery(Guid Id) : BaseQueryById<PersonnelDTO, Guid>(Id);
public sealed record GetPersonnelPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<PersonnelDTO>(PageNumber, PageSize);
