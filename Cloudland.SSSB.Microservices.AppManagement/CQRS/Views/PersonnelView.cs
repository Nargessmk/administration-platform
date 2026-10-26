using Cloudland.APM.DTO;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

// View queries
public record GetPersonnelViewAllQuery
    : IRequest<ResultDTO<List<PersonnelViewDTO>>>;

public record GetPersonnelViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<PersonnelViewDTO>>>;

public record GetPersonnelViewByIdQuery(Guid Id) 
    : IRequest<ResultDTO<PersonnelViewDTO>>;

public record GetPersonnelViewChildrenByParentIdPagingQuery(Guid ParentId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<PersonnelViewDTO>>>;

public record GetPersonnelViewsByOrganizationalPositionIdQuery(Guid OrganizationalPositionId)
    : IRequest<ResultDTO<List<PersonnelViewDTO>>>;

public record GetPersonnelViewsByOrganizationalPositionIdPagingQuery(Guid OrganizationalPositionId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<PersonnelViewDTO>>>;

public record GetPersonnelViewsByOrganizationalChartIdQuery(Guid OrganizationalChartId)
    : IRequest<ResultDTO<List<PersonnelViewDTO>>>;

public record GetPersonnelViewsByOrganizationalChartIdPagingQuery(Guid OrganizationalChartId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<PersonnelViewDTO>>>;

public record GetPersonnelViewsByContractTypeIdQuery(Guid ContractTypeId)
    : IRequest<ResultDTO<List<PersonnelViewDTO>>>;

public record GetPersonnelViewsByContractTypeIdPagingQuery(Guid ContractTypeId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<PersonnelViewDTO>>>;

public record GetPersonnelViewsByAccountNumberIdForReservationRequestPersonnelPagingQuery(Guid AccountNumberId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<PersonnelViewDTO>>>;
// Recursive queries
public record GetPersonnelRecursiveAllQuery(Guid RootId) 
    : IRequest<ResultDTO<List<ConstantRecursiveDTO>>>;

public record GetPersonnelRecursivePagingQuery(Guid RootId, int PageNumber, int PageSize) 
    : IRequest<ResultDTO<PaginatedResult<ConstantRecursiveDTO>>>;

public record GetPersonnelRecursiveByIdQuery(Guid RootId, Guid ChildId) 
    : IRequest<ResultDTO<ConstantRecursiveDTO>>;

public record SearchPersonnelByNameQuery(string SearchTerm, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<PersonnelViewDTO>>>;