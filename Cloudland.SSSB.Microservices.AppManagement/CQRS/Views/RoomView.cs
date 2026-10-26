using Cloudland.APM.DTO;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

// View queries
public record GetRoomViewAllQuery
    : IRequest<ResultDTO<List<RoomViewDTO>>>;

public record GetRoomViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<RoomViewDTO>>>;

public record GetRoomViewByIdQuery(Guid Id) 
    : IRequest<ResultDTO<RoomViewDTO>>;

public record GetRoomViewChildrenByParentIdPagingQuery(Guid ParentId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<RoomViewDTO>>>;

public record GetRoomViewsByLocationIdQuery(Guid LocationId)
    : IRequest<ResultDTO<List<RoomViewDTO>>>;

public record GetRoomViewsByLocationIdPagingQuery(Guid LocationId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<RoomViewDTO>>>;
