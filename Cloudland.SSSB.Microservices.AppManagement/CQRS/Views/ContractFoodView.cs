using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;

public record GetContractFoodViewAllQuery
    : IRequest<ResultDTO<List<ContractFoodViewDTO>>>;

public record GetContractFoodViewAllPagingQuery(int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ContractFoodViewDTO>>>;

public record GetContractFoodViewByIdQuery(Guid Id)
    : IRequest<ResultDTO<ContractFoodViewDTO>>;

public record GetContractFoodViewAllByContractIdQuery(Guid ContractId)
    : IRequest<ResultDTO<List<ContractFoodViewDTO>>>;

public record GetContractFoodViewPagingByContractIdQuery(Guid ContractId, int PageNumber, int PageSize)
    : IRequest<ResultDTO<PaginatedResult<ContractFoodViewDTO>>>;


