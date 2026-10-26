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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.AccountNumber.Query;

public sealed class GetAccountNumberPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.AccountNumber> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<GetAccountNumberPagingQueryHandler> logger)
    : BaseHandler<Models.AccountNumber, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetAccountNumberPagingQuery, ResultDTO<PaginatedResult<AccountNumberDTO>>>
{
    private readonly ILogger<GetAccountNumberPagingQueryHandler> _logger = logger;

    public async Task<ResultDTO<PaginatedResult<AccountNumberDTO>>> Handle(GetAccountNumberPagingQuery request, CancellationToken cancellationToken)
    {
        
        try
        {
            // Get the database context
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext 
                             ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");
            
            var query = _dbContext.AccountNumbers
                .Where(x => !x.Disabled && !x.hidden)
                .OrderBy(x => x.title).AsQueryable();

            //pagination
            var paginatedResult = new PaginatedResult<AccountNumberDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.Paginate(query.Select(x => _mapper.Map<AccountNumberDTO>(x)));

            return ResultDTO<PaginatedResult<AccountNumberDTO>>.Success(
                paginatedResult,
                "لیست شماره حساب با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<AccountNumberDTO>>.Failure(
                $"خطا در دریافت لیست شماره حساب ها: {ex.Message}",
                []
            );
        }
    }
}
