using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

public record GetContractViewAllQuery
    : IRequest<ResultDTO<List<ContractViewDTO>>>;

public record GetContractViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ContractViewDTO>>>;

public record GetContractViewByIdQuery(Guid Id)
    : IRequest<ResultDTO<ContractViewDTO>>;

public record GetContractViewChildrenByParentIdPagingQuery(Guid ParentId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ContractViewDTO>>>;

public record GetContractViewByLocationIdQuery(Guid LocationId)
    : IRequest<ResultDTO<List<ContractViewDTO>>>;

public record GetContractViewsByContractorIdQuery(Guid ContractorId)
    : IRequest<ResultDTO<List<ContractViewDTO>>>;

public record GetContractViewsByContractorIdPagingQuery(Guid ContractorId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ContractViewDTO>>>;

