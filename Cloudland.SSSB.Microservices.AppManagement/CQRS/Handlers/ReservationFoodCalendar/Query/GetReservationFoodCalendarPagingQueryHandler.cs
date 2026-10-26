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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationFoodCalendar.Query;

public sealed class GetReservationFoodCalendarPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationFoodCalendar> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ReservationFoodCalendar, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
        distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetReservationFoodCalendarPagingQuery, ResultDTO<PaginatedResult<ReservationFoodCalendarDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<ReservationFoodCalendarDTO>>> Handle(GetReservationFoodCalendarPagingQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.MultipleResultQuery();
        var entities = await _repository.SearchAsync(query, cancellationToken);

        var paginatedResult = new PaginatedResult<ReservationFoodCalendarDTO>(request.PageNumber, request.PageSize);
        await paginatedResult.PaginateFromEnumerable(entities.Select(x => _mapper.Map<ReservationFoodCalendarDTO>(x)));

        return ResultDTO<PaginatedResult<ReservationFoodCalendarDTO>>.Success(paginatedResult);
    }
}
