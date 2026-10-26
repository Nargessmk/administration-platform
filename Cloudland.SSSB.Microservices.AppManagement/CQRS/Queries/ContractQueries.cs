using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetContractByIdQuery(Guid Id) : BaseQueryById<ContractDTO, Guid>(Id);
public sealed record GetContractPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<ContractDTO>(PageNumber, PageSize);

public sealed record GetContractRecursive()
    : BaseQuery<ContractRecursiveDTO>();
