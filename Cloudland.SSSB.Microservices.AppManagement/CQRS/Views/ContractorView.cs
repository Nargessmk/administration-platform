using Cloudland.APM.DTO;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

// View queries
public record GetContractorViewAllQuery
    : IRequest<ResultDTO<List<ContractorViewDTO>>>;

public record GetContractorViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ContractorViewDTO>>>;

public record GetContractorViewByIdQuery(Guid Id)
    : IRequest<ResultDTO<ContractorViewDTO>>;

// Recursive queries
public record GetContractorRecursiveAllQuery(Guid RootId)
    : IRequest<ResultDTO<List<ConstantRecursiveDTO>>>;

public record GetContractorRecursivePagingQuery(Guid RootId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ConstantRecursiveDTO>>>;

public record GetContractorRecursiveByIdQuery(Guid RootId, Guid ChildId)
    : IRequest<ResultDTO<ConstantRecursiveDTO>>;


