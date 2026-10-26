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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.WeekDay.View;

public sealed class GetWeekDayRecursivePagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.WeekDay> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.WeekDay, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetWeekDayRecursivePagingQuery, ResultDTO<PaginatedResult<ConstantRecursiveDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<ConstantRecursiveDTO>>> Handle(
        GetWeekDayRecursivePagingQuery request,
        CancellationToken cancellationToken)
    {
        var _dbContext = _unitOfWork.DbContext as SSSBDBContext
            ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

        var query = _dbContext
            .GetWeekDayRecursive(request.RootId)
            .AsQueryable();

        var paginatedResult = new PaginatedResult<ConstantRecursiveDTO>(request.PageNumber, request.PageSize);
        await paginatedResult.Paginate(query.Select(x => _mapper.Map<ConstantRecursiveDTO>(x)));

        return ResultDTO<PaginatedResult<ConstantRecursiveDTO>>.Success(
            paginatedResult,
            "ساختار تقویم با صفحه‌بندی با موفقیت دریافت شد"
        );
    }
}


