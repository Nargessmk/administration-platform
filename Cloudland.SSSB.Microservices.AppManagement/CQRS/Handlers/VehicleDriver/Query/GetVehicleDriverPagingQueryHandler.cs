using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.VehicleDriver.Query;

public sealed class GetVehicleDriverDriverPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.VehicleDriver> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.VehicleDriver, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetVehicleDriverPagingQuery, ResultDTO<PaginatedResult<VehicleDriverDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<VehicleDriverDTO>>> Handle(GetVehicleDriverPagingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext 
                ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");
            
            var query = _dbContext.VehicleDrivers
                .Where(x => !x.Disabled && !x.hidden)
                .OrderBy(x => x.title).AsQueryable();

            var paginatedResult = new PaginatedResult<VehicleDriverDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.Paginate(query.Select(x => _mapper.Map<VehicleDriverDTO>(x)));

            return ResultDTO<PaginatedResult<VehicleDriverDTO>>.Success(
                paginatedResult,
                "لیست راننده های  وسیله نقلیه با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<VehicleDriverDTO>>.Failure(
                $"خطا در دریافت لیست راننده های وسیله نقلیه: {ex.Message}",
                new List<string>()
            );
        }
    }
}
