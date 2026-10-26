using AutoMapper;
using Cloudland.APM.DTO;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Food.View;

public sealed class GetFoodRecursivePagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Food> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Food, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetFoodRecursivePagingQuery, ResultDTO<PaginatedResult<ConstantRecursiveDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<ConstantRecursiveDTO>>> Handle(
        GetFoodRecursivePagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext
                ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            // Query from the recursive function
            var query = _dbContext
                .GetFoodRecursive(request.RootId)
                .AsQueryable();

            // Apply pagination
            var paginatedResult = new PaginatedResult<ConstantRecursiveDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.Paginate(query.Select(x => _mapper.Map<ConstantRecursiveDTO>(x)));

            return ResultDTO<PaginatedResult<ConstantRecursiveDTO>>.Success(
                paginatedResult,
                "زیرساخت غذاها با صفحه‌بندی با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ConstantRecursiveDTO>>.Failure(
                $"خطا در دریافت زیرساخت غذاها: {ex.Message}",
                []
            );
        }
    }
}
