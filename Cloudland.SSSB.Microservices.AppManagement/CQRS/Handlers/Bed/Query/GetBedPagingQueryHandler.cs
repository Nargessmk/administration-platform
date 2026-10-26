using AutoMapper;
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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Bed.Query;

public sealed class GetBedPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Bed> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Bed, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetBedPagingQuery, ResultDTO<PaginatedResult<BedDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<BedDTO>>> Handle(GetBedPagingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _repository
                .MultipleResultQuery()
                .AndFilter(x => !x.Disabled && !x.hidden)
                .OrderBy(x => x.title);

            var entities = await _repository.SearchAsync(query, cancellationToken);
            var mappedEntities = entities.Select(x => _mapper.Map<BedDTO>(x));

            var paginatedResult = new PaginatedResult<BedDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(mappedEntities);

            return ResultDTO<PaginatedResult<BedDTO>>.Success(
                paginatedResult,
                "لیست تخت ها با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<BedDTO>>.Failure(
                $"خطا در دریافت لیست  تخت ها: {ex.Message}",
                []
            );
        }
    }
}
