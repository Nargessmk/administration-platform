using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetContractFoodLocationByIdQuery(Guid Id) : BaseQueryById<ContractFoodLocationDTO, Guid>(Id);
public sealed record GetContractFoodLocationPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<ContractFoodLocationDTO>(PageNumber, PageSize);
