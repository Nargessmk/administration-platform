using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetReservationRequestPersonByIdQuery(Guid Id) : BaseQueryById<ReservationRequestPersonDTO, Guid>(Id);
public sealed record GetReservationRequestPersonPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<ReservationRequestPersonDTO>(PageNumber, PageSize);




