using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetAccountNumberByIdQuery(Guid Id) : BaseQueryById<AccountNumberDTO, Guid>(Id);
public sealed record GetAccountNumberPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<AccountNumberDTO>(PageNumber, PageSize);
