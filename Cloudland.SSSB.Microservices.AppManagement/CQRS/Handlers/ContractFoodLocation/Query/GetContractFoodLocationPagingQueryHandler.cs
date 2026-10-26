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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFoodLocation.Query;

public sealed class GetContractFoodLocationPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFoodLocation> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractFoodLocation, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractFoodLocationPagingQuery, ResultDTO<PaginatedResult<ContractFoodLocationDTO>>>
{

    public async Task<ResultDTO<PaginatedResult<ContractFoodLocationDTO>>> Handle(GetContractFoodLocationPagingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext 
                             ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            var query = _dbContext.ContractFoodLocations
                .Where(x => !x.Disabled && !x.hidden)
                .OrderBy(x => x.CreateAt)
                .AsQueryable();

            var paginatedResult = new PaginatedResult<ContractFoodLocationDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.Paginate(query.Select(x => _mapper.Map<ContractFoodLocationDTO>(x)));

            return ResultDTO<PaginatedResult<ContractFoodLocationDTO>>.Success(
                paginatedResult,
                "لیست روابط غذای قرارداد-مکان با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ContractFoodLocationDTO>>.Failure(
                $"خطا در دریافت لیست روابط غذای قرارداد-مکان: {ex.Message}",
                []
            );
        }
    }
}
