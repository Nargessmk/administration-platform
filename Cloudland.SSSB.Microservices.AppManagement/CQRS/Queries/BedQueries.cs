using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetBedByIdQuery(Guid Id) : BaseQueryById<BedDTO, Guid>(Id);
public sealed record GetBedPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<BedDTO>(PageNumber, PageSize);

// Room-based bed queries
public sealed record GetBedsByRoomIdQuery(Guid RoomId) : BaseQueryListById<BedDTO, Guid>(RoomId);
public sealed record GetBedsByRoomIdPagingQuery(Guid RoomId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<BedDTO>>>;
