using Cloudland.APM.DTO;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetReservationFoodCalendarByIdQuery(Guid Id) : BaseQueryById<ReservationFoodCalendarDTO, Guid>(Id);
public sealed record GetReservationFoodCalendarPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<ReservationFoodCalendarDTO>(PageNumber, PageSize);
public sealed record GetShiftWorkerRequestsByReservationRequestHeaderIdQuery(Guid ReservationRequestHeaderId) 
    : IRequest<ResultDTO<List<ShiftWorkerRequestDTO>>>;
