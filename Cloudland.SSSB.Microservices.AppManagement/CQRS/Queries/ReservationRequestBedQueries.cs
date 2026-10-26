using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetReservationRequestBedByIdQuery(Guid Id) : BaseQueryById<ReservationRequestBedDTO, Guid>(Id);
public sealed record GetReservationRequestBedPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<ReservationRequestBedDTO>(PageNumber, PageSize);




