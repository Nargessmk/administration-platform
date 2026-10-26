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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Contractor.View;

public sealed class GetContractorRecursivePagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Contractor> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Contractor, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractorRecursivePagingQuery, ResultDTO<PaginatedResult<ConstantRecursiveDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<ConstantRecursiveDTO>>> Handle(
        GetContractorRecursivePagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext
                ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            var query = _dbContext
                .GetContractorRecursive(request.RootId)
                .AsQueryable();

            var paginated = new PaginatedResult<ConstantRecursiveDTO>(request.PageNumber, request.PageSize);
            await paginated.Paginate(query.Select(x => _mapper.Map<ConstantRecursiveDTO>(x)));

            return ResultDTO<PaginatedResult<ConstantRecursiveDTO>>.Success(
                paginated,
                "زیرساخت پیمانکاران با صفحه‌بندی با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ConstantRecursiveDTO>>.Failure(
                $"خطا در دریافت زیرساخت پیمانکاران: {ex.Message}",
                []
            );
        }
    }
}


