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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Contractor.View;

public sealed class GetContractorViewAllPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Contractor> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ContractorView> contractorViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Contractor, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractorViewAllPagingQuery, ResultDTO<PaginatedResult<ContractorViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<ContractorViewDTO>>> Handle(
        GetContractorViewAllPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = contractorViewRepository.MultipleResultQuery();

            var entities = await contractorViewRepository.SearchAsync(query, cancellationToken);

            var paginatedResult = new PaginatedResult<ContractorViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(entities.Select(x => _mapper.Map<ContractorViewDTO>(x)));

            return ResultDTO<PaginatedResult<ContractorViewDTO>>.Success(
                paginatedResult,
                "لیست پیمانکاران با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ContractorViewDTO>>.Failure(
                $"خطا در دریافت لیست پیمانکاران: {ex.Message}",
                []
            );
        }
    }
}


