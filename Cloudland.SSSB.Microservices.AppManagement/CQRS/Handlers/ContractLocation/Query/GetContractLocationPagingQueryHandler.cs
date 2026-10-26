using AutoMapper;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractLocation.Query;

public sealed class GetContractLocationPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractLocation> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractLocation, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractLocationPagingQuery, ResultDTO<PaginatedResult<ContractLocationDTO>>>
{

    public async Task<ResultDTO<PaginatedResult<ContractLocationDTO>>> Handle(GetContractLocationPagingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext 
                             ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            var query = _dbContext.ContractLocations
                .Where(x => !x.Disabled && !x.hidden)
                .OrderBy(x => x.CreateAt)
                .AsQueryable();

            var paginatedResult = new PaginatedResult<ContractLocationDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.Paginate(query.Select(x => _mapper.Map<ContractLocationDTO>(x)));

            return ResultDTO<PaginatedResult<ContractLocationDTO>>.Success(
                paginatedResult,
                "لیست مکان‌های قرارداد با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ContractLocationDTO>>.Failure(
                $"خطا در دریافت لیست مکان‌های قرارداد: {ex.Message}",
                []
            );
        }
    }
}


