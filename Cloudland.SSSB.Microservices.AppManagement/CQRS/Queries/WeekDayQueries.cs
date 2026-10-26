using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetWeekDayByIdQuery(Guid Id) : BaseQueryById<WeekDayDTO, Guid>(Id);
public sealed record GetWeekDayPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<WeekDayDTO>(PageNumber, PageSize);


