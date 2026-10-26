using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetContractFoodByIdQuery(Guid Id) : BaseQueryById<ContractFoodDTO, Guid>(Id);
public sealed record GetContractFoodPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<ContractFoodDTO>(PageNumber, PageSize);


