using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

public record GetReservationRequestPersonViewAllQuery
    : IRequest<ResultDTO<List<ReservationRequestPersonViewDTO>>>;

public record GetReservationRequestPersonViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ReservationRequestPersonViewDTO>>>;

public record GetReservationRequestPersonViewByIdQuery(Guid Id) 
    : IRequest<ResultDTO<ReservationRequestPersonViewDTO>>;


public record GetReservationRequestPersonViewByReservationRequestHeaderIdQuery(Guid ReservationRequestId) 
    : IRequest<ResultDTO<List<ReservationRequestPersonViewDTO>>>;

public record GetReservationRequestPersonViewByReservationRequestHeaderIdPagingQuery(Guid ReservationRequestId, int PageNumber, int PageSize) 
    : IRequest<ResultDTO<PaginatedResult<ReservationRequestPersonViewDTO>>>;

