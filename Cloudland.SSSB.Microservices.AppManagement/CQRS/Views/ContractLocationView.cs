using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

public record GetContractLocationViewAllQuery
    : IRequest<ResultDTO<List<ContractLocationViewDTO>>>;

public record GetContractLocationViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ContractLocationViewDTO>>>;

public record GetContractLocationViewByIdQuery(Guid Id)
    : IRequest<ResultDTO<ContractLocationViewDTO>>;

public record GetContractLocationViewAllByContractIdQuery(Guid ContractId)
    : IRequest<ResultDTO<List<ContractLocationViewDTO>>>;

public record GetContractLocationViewPagingByContractIdQuery(Guid ContractId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ContractLocationViewDTO>>>;


