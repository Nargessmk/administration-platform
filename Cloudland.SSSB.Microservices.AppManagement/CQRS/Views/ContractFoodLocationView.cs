using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

public record GetContractFoodLocationViewAllQuery
    : IRequest<ResultDTO<List<ContractFoodLocationViewDTO>>>;

public record GetContractFoodLocationViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ContractFoodLocationViewDTO>>>;

public record GetContractFoodLocationViewByIdQuery(Guid Id)
    : IRequest<ResultDTO<ContractFoodLocationViewDTO>>;

public record GetContractFoodLocationViewAllByContractIdQuery(Guid ContractId)
    : IRequest<ResultDTO<List<ContractFoodLocationViewDTO>>>;

public record GetContractFoodLocationViewPagingByContractIdQuery(Guid ContractId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ContractFoodLocationViewDTO>>>;

public record GetContractFoodLocationViewAllByLocationIdQuery(Guid LocationId)
    : IRequest<ResultDTO<List<ContractFoodLocationViewDTO>>>;

public record GetContractFoodLocationViewPagingByLocationIdQuery(Guid LocationId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ContractFoodLocationViewDTO>>>;

public record GetContractFoodLocationViewPagingByContractIdAndLocationIdQuery(Guid ContractId, Guid LocationId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ContractFoodLocationViewDTO>>>;

public record GetContractFoodLocationViewAllByContractIdAndLocationIdQuery(Guid ContractId, Guid LocationId)
    : IRequest<ResultDTO<List<ContractFoodLocationViewDTO>>>;
