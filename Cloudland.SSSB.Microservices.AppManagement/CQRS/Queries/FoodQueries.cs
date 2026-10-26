using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetFoodByIdQuery(Guid Id) : BaseQueryById<FoodDTO, Guid>(Id);
public sealed record GetFoodPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<FoodDTO>(PageNumber, PageSize);
