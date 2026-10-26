using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

public record GetReservationRequestLetterViewAllQuery
    : IRequest<ResultDTO<List<ReservationRequestLetterViewDTO>>>;

public record GetReservationRequestLetterViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ReservationRequestLetterViewDTO>>>;

public record GetReservationRequestLetterViewByIdQuery(Guid Id) 
    : IRequest<ResultDTO<ReservationRequestLetterViewDTO>>;

public record GetReservationRequestLetterViewByReservationRequestHeaderIdQuery(Guid ReservationRequestId) 
    : IRequest<ResultDTO<ReservationRequestLetterViewDTO>>;

public record GetReservationRequestLetterViewByReservationRequestHeaderIdPagingQuery(Guid ReservationRequestId, int PageNumber, int PageSize) 
    : IRequest<ResultDTO<PaginatedResult<ReservationRequestLetterViewDTO>>>;




