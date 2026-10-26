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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractLocation.View;

public sealed class GetContractLocationViewAllPagingQueryHandler(
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
        IRequestHandler<GetContractLocationViewAllPagingQuery, ResultDTO<PaginatedResult<ContractLocationViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<ContractLocationViewDTO>>> Handle(
        GetContractLocationViewAllPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = contractLocationViewRepository.MultipleResultQuery();

            var entities = await contractLocationViewRepository.SearchAsync(query, cancellationToken);

            var paginatedResult = new PaginatedResult<ContractLocationViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(entities.Select(x => _mapper.Map<ContractLocationViewDTO>(x)));

            return ResultDTO<PaginatedResult<ContractLocationViewDTO>>.Success(
                paginatedResult,
                "لیست با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ContractLocationViewDTO>>.Failure(
                $"خطا در دریافت لیست: {ex.Message}",
                []
            );
        }
    }
}



