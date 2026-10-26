using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetReservationRequestLetterByIdQuery(Guid Id) : BaseQueryById<ReservationRequestLetterDTO, Guid>(Id);
public sealed record GetReservationRequestLetterPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<ReservationRequestLetterDTO>(PageNumber, PageSize);




