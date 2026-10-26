using Cloudland.APM.DTO;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

// View queries
public record GetOrganizationalChartViewAllQuery
    : IRequest<ResultDTO<List<OrganizationalChartViewDTO>>>;

public record GetOrganizationalChartViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<OrganizationalChartViewDTO>>>;

public record GetOrganizationalChartViewByIdQuery(Guid Id) 
    : IRequest<ResultDTO<OrganizationalChartViewDTO>>;

public record GetOrganizationalChartViewChildrenByParentIdPagingQuery(Guid ParentId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<OrganizationalChartViewDTO>>>;

// Recursive queries
public record GetOrganizationalChartRecursiveAllQuery(Guid RootId) 
    : IRequest<ResultDTO<List<ConstantRecursiveDTO>>>;

public record GetOrganizationalChartRecursivePagingQuery(Guid RootId,int PageNumber, int PageSize) 
    : IRequest<ResultDTO<PaginatedResult<ConstantRecursiveDTO>>>;

public record GetOrganizationalChartRecursiveByIdQuery(Guid RootId, Guid ChildId) 
    : IRequest<ResultDTO<ConstantRecursiveDTO>>;
