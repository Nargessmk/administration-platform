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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Contractor.Query;

public sealed class GetContractorPagingQueryHandler(
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
        IRequestHandler<GetContractorPagingQuery, ResultDTO<PaginatedResult<ContractorDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<ContractorDTO>>> Handle(GetContractorPagingQuery request, CancellationToken cancellationToken)
    {
        
        try
        {
            // Get the database context
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext 
                             ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");
            
            var query = _dbContext.Contractors
                .Where(x => !x.Disabled && !x.hidden)
                .OrderBy(x => x.title).AsQueryable();

            //pagination
            var paginatedResult = new PaginatedResult<ContractorDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.Paginate(query.Select(x => _mapper.Map<ContractorDTO>(x)));

            return ResultDTO<PaginatedResult<ContractorDTO>>.Success(
                paginatedResult,
                "لیست پیمانکاران با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ContractorDTO>>.Failure(
                $"خطا در دریافت لیست شماره پیمانکاران: {ex.Message}",
                []
            );
        }
    }
}
