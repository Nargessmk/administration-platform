using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetRoomByIdQuery(Guid Id) : BaseQueryById<RoomDTO, Guid>(Id);
public sealed record GetRoomPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<RoomDTO>(PageNumber, PageSize);

// Location-based room queries
public sealed record GetRoomsByLocationIdQuery(Guid LocationId) : BaseQueryListById<RoomDTO, Guid>(LocationId);
public sealed record GetRoomsByLocationIdPagingQuery(Guid LocationId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<RoomDTO>>>;
