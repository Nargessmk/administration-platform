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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFood.Query;

public sealed class GetContractFoodPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFood> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractFood, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractFoodPagingQuery, ResultDTO<PaginatedResult<ContractFoodDTO>>>
{

    public async Task<ResultDTO<PaginatedResult<ContractFoodDTO>>> Handle(GetContractFoodPagingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext 
                             ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            var query = _dbContext.ContractFoods
                .Where(x => !x.Disabled && !x.hidden)
                .OrderBy(x => x.CreateAt)
                .AsQueryable();

            var paginatedResult = new PaginatedResult<ContractFoodDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.Paginate(query.Select(x => _mapper.Map<ContractFoodDTO>(x)));

            return ResultDTO<PaginatedResult<ContractFoodDTO>>.Success(
                paginatedResult,
                "لیست غذاهای قرارداد با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ContractFoodDTO>>.Failure(
                $"خطا در دریافت لیست غذاهای قرارداد: {ex.Message}",
                []
            );
        }
    }
}


