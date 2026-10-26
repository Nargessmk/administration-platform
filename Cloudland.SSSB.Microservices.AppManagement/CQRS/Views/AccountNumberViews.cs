using Cloudland.APM.DTO;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

// View queries
public record GetAccountNumberViewAllQuery
    : IRequest<ResultDTO<List<AccountNumberViewDTO>>>;

public record GetAccountNumberViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<AccountNumberViewDTO>>>;

public record GetAccountNumberViewByIdQuery(Guid Id) 
    : IRequest<ResultDTO<AccountNumberViewDTO>>;

public record GetAccountNumberViewsByOrganizationalChartIdQuery(Guid OrganizationalChartId)
    : IRequest<ResultDTO<List<AccountNumberViewDTO>>>;

public record GetAccountNumberViewsByOrganizationalChartIdPagingQuery(Guid OrganizationalChartId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<AccountNumberViewDTO>>>;
