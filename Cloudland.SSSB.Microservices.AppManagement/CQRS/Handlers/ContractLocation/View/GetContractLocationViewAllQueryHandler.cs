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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractLocation.View;

public sealed class GetContractLocationViewAllQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractLocation> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ContractLocationView> contractLocationViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractLocation, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractLocationViewAllQuery, ResultDTO<List<ContractLocationViewDTO>>>
{
    public async Task<ResultDTO<List<ContractLocationViewDTO>>> Handle(
        GetContractLocationViewAllQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = contractLocationViewRepository.MultipleResultQuery();

            var entities = await contractLocationViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<ContractLocationViewDTO>>(entities);

            return ResultDTO<List<ContractLocationViewDTO>>.Success(
                dtos,
                "لیست کامل مکان‌های قرارداد با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<ContractLocationViewDTO>>.Failure(
                $"خطا در دریافت لیست مکان‌های قرارداد: {ex.Message}",
                new List<ContractLocationViewDTO>()
            );
        }
    }
}



