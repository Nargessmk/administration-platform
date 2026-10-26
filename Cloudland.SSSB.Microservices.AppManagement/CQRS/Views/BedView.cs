using Cloudland.APM.DTO;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

// View queries
public record GetBedViewAllQuery
    : IRequest<ResultDTO<List<BedViewDTO>>>;

public record GetBedViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<BedViewDTO>>>;

public record GetBedViewByIdQuery(Guid Id) 
    : IRequest<ResultDTO<BedViewDTO>>;

public record GetBedViewChildrenByParentIdPagingQuery(Guid ParentId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<BedViewDTO>>>;

public record GetBedViewsByRoomIdQuery(Guid RoomId)
    : IRequest<ResultDTO<List<BedViewDTO>>>;

public record GetBedViewsByRoomIdPagingQuery(Guid RoomId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<BedViewDTO>>>;
