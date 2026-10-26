using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Queries.Base;
using MediatR;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Queries;

public sealed record GetCalendarByIdQuery(Guid Id) : BaseQueryById<CalendarDTO, Guid>(Id);

public sealed record GetCalendarPagingQuery(int Year, int PageNumber, int PageSize) : BaseQueryPaging<CalendarDTO>(PageNumber, PageSize);

