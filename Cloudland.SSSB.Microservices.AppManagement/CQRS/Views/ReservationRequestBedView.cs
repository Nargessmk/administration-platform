using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

public record GetReservationRequestBedViewAllQuery
    : IRequest<ResultDTO<List<ReservationRequestBedViewDTO>>>;

public record GetReservationRequestBedViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ReservationRequestBedViewDTO>>>;

public record GetReservationRequestBedViewByIdQuery(Guid Id) 
    : IRequest<ResultDTO<ReservationRequestBedViewDTO>>;

public record GetReservationRequestBedViewByReservationRequestHeaderIdQuery(Guid ReservationRequestId) 
    : IRequest<ResultDTO<List<ReservationRequestBedViewDTO>>>;

public record GetReservationRequestBedViewByReservationRequestHeaderIdPagingQuery(Guid ReservationRequestId, int PageNumber, int PageSize) 
    : IRequest<ResultDTO<PaginatedResult<ReservationRequestBedViewDTO>>>;





