using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetContractFoodCalendarByIdQuery(Guid Id) : BaseQueryById<ContractFoodCalendarDTO, Guid>(Id);
public sealed record GetContractFoodCalendarPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<ContractFoodCalendarDTO>(PageNumber, PageSize);


