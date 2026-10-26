using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetContractLocationByIdQuery(Guid Id) : BaseQueryById<ContractLocationDTO, Guid>(Id);
public sealed record GetContractLocationPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<ContractLocationDTO>(PageNumber, PageSize);


