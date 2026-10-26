using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFoodLocation.View;

public sealed class GetContractFoodLocationViewPagingByContractIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFoodLocation> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ContractFoodLocationView> contractFoodLocationViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractFoodLocation, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractFoodLocationViewPagingByContractIdQuery, ResultDTO<PaginatedResult<ContractFoodLocationViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<ContractFoodLocationViewDTO>>> Handle(
        GetContractFoodLocationViewPagingByContractIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = contractFoodLocationViewRepository.MultipleResultQuery()
                .AndFilter(x => x.ContractId == request.ContractId);
            var entities = await contractFoodLocationViewRepository.SearchAsync(query, cancellationToken);

            var paginatedResult = new PaginatedResult<ContractFoodLocationViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(entities.Select(x => _mapper.Map<ContractFoodLocationViewDTO>(x)));

            return ResultDTO<PaginatedResult<ContractFoodLocationViewDTO>>.Success(paginatedResult, "لیست با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ContractFoodLocationViewDTO>>.Failure($"خطا در دریافت لیست: {ex.Message}", []);
        }
    }
}


