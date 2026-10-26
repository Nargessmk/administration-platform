using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

// View queries
public record GetReservationRequestHeaderViewAllQuery
    : IRequest<ResultDTO<List<ReservationRequestHeaderViewDTO>>>;

public record GetReservationRequestHeaderViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ReservationRequestHeaderViewDTO>>>;

public record GetReservationRequestHeaderViewByIdQuery(Guid Id) 
    : IRequest<ResultDTO<ReservationRequestHeaderViewDTO>>;

public record GetReservationRequestHeaderViewCountByDateQuery(string PersianDate)
    : IRequest<ResultDTO<int>>;

public record GetReservationRequestHeaderViewCountTodayQuery
    : IRequest<ResultDTO<int>>;

public record GetReservationRequestHeaderViewCountByDateForUserQuery(string PersianDate)
    : IRequest<ResultDTO<int>>;

public record GetReservationRequestHeaderViewCountTodayForUserQuery
    : IRequest<ResultDTO<int>>;



