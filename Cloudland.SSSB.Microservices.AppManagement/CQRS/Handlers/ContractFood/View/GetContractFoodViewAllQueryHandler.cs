using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFood.View;

public sealed class GetContractFoodViewAllQueryHandler(
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
        IRequestHandler<GetContractFoodViewAllQuery, ResultDTO<List<ContractFoodViewDTO>>>
{
    public async Task<ResultDTO<List<ContractFoodViewDTO>>> Handle(
        GetContractFoodViewAllQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = contractFoodViewRepository.MultipleResultQuery();

            var entities = await contractFoodViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<ContractFoodViewDTO>>(entities);

            return ResultDTO<List<ContractFoodViewDTO>>.Success(
                dtos,
                "لیست کامل غذاهای قرارداد با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<ContractFoodViewDTO>>.Failure(
                $"خطا در دریافت لیست: {ex.Message}",
                new List<ContractFoodViewDTO>()
            );
        }
    }

}


