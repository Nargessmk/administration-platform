using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

public record GetReservationFoodCalendarViewAllQuery
    : IRequest<ResultDTO<List<ReservationFoodCalendarViewDTO>>>;

public record GetReservationFoodCalendarViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ReservationFoodCalendarViewDTO>>>;

public record GetReservationFoodCalendarViewByIdQuery(Guid Id) 
    : IRequest<ResultDTO<ReservationFoodCalendarViewDTO>>;

public record GetReservationFoodCalendarViewByReservationRequestHeaderIdQuery(Guid ReservationRequestId) 
    : IRequest<ResultDTO<List<ReservationFoodCalendarViewDTO>>>;

public record GetReservationFoodCalendarViewByReservationRequestHeaderIdPagingQuery(Guid ReservationRequestId, int PageNumber, int PageSize) 
    : IRequest<ResultDTO<PaginatedResult<ReservationFoodCalendarViewDTO>>>;
