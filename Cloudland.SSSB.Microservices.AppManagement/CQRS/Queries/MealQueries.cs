using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetMealByIdQuery(Guid Id) : BaseQueryById<MealDTO, Guid>(Id);
public sealed record GetMealPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<MealDTO>(PageNumber, PageSize);
