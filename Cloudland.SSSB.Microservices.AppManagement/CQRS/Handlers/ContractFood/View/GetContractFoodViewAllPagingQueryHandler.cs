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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFood.View;

public sealed class GetContractFoodViewAllPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFood> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ContractFoodView> contractFoodViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractFood, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractFoodViewAllPagingQuery, ResultDTO<PaginatedResult<ContractFoodViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<ContractFoodViewDTO>>> Handle(
        GetContractFoodViewAllPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = contractFoodViewRepository.MultipleResultQuery();

            var entities = await contractFoodViewRepository.SearchAsync(query, cancellationToken);

            var paginatedResult = new PaginatedResult<ContractFoodViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(entities.Select(x => _mapper.Map<ContractFoodViewDTO>(x)));

            return ResultDTO<PaginatedResult<ContractFoodViewDTO>>.Success(
                paginatedResult,
                "لیست با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ContractFoodViewDTO>>.Failure(
                $"خطا در دریافت لیست: {ex.Message}",
                []
            );
        }
    }

}


