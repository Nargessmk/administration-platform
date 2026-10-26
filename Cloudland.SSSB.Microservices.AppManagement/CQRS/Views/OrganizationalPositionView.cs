using Cloudland.APM.DTO;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

// View queries
public record GetOrganizationalPositionViewAllQuery 
    : IRequest<ResultDTO<List<OrganizationalPositionViewDTO>>>;

public record GetOrganizationalPositionViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<OrganizationalPositionViewDTO>>>;

public record GetOrganizationalPositionViewByIdQuery(Guid Id) 
    : IRequest<ResultDTO<OrganizationalPositionViewDTO>>;

public record GetOrganizationalPositionViewChildrenByParentIdPagingQuery(Guid ParentId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<OrganizationalPositionViewDTO>>>;

// Recursive queries
public record GetOrganizationalPositionRecursiveAllQuery(Guid RootId) 
    : IRequest<ResultDTO<List<ConstantRecursiveDTO>>>;

public record GetOrganizationalPositionRecursivePagingQuery(Guid RootId, int PageNumber, int PageSize) 
    : IRequest<ResultDTO<PaginatedResult<ConstantRecursiveDTO>>>;

public record GetOrganizationalPositionRecursiveByIdQuery(Guid RootId, Guid ChildId) 
    : IRequest<ResultDTO<ConstantRecursiveDTO>>;
