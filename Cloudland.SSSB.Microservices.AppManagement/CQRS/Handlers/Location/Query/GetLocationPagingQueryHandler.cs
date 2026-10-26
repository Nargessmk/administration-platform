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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Location.Query;

public sealed class GetLocationPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Location> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Location, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetLocationPagingQuery, ResultDTO<PaginatedResult<LocationDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<LocationDTO>>> Handle(GetLocationPagingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the database context
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext 
                             ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");
            
            var query = _dbContext.Locations
                .Where(x => !x.Disabled && !x.hidden)
                .OrderBy(x => x.title).AsQueryable();

            //pagination
            var paginatedResult = new PaginatedResult<LocationDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.Paginate(query.Select(x => _mapper.Map<LocationDTO>(x)));

            return ResultDTO<PaginatedResult<LocationDTO>>.Success(
                paginatedResult,
                "لیست محل ها با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<LocationDTO>>.Failure(
                $"خطا در دریافت لیست  محل ها: {ex.Message}",
                []
            );
        }
    }
}
