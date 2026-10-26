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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestHeader.Query;

public sealed class GetReservationRequestHeaderPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestHeader> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ReservationRequestHeader, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetReservationRequestHeaderPagingQuery, ResultDTO<PaginatedResult<ReservationRequestHeaderDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<ReservationRequestHeaderDTO>>> Handle(GetReservationRequestHeaderPagingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _repository
                .MultipleResultQuery()
                .AndFilter(x => !x.Disabled && !x.hidden)
                .OrderBy(x => x.CreateAt);

            var entities = await _repository.SearchAsync(query, cancellationToken);
            var mappedEntities = entities.Select(x => _mapper.Map<ReservationRequestHeaderDTO>(x));

            var paginatedResult = new PaginatedResult<ReservationRequestHeaderDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(mappedEntities);

            return ResultDTO<PaginatedResult<ReservationRequestHeaderDTO>>.Success(
                paginatedResult,
                "لیست درخواست های رزرو با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ReservationRequestHeaderDTO>>.Failure(
                $"خطا در دریافت لیست درخواست های رزرو: {ex.Message}",
                []
            );
        }
    }
}



