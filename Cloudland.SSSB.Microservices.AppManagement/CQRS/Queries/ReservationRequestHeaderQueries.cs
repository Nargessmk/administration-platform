using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetReservationRequestHeaderByIdQuery(Guid Id) : BaseQueryById<ReservationRequestHeaderDTO, Guid>(Id);
public sealed record GetReservationRequestHeaderPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<ReservationRequestHeaderDTO>(PageNumber, PageSize);



