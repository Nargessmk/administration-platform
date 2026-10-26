using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.WeekDay.View;

public sealed class GetWeekDayViewAllPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.WeekDay> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<WeekDayView> WeekDayViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.WeekDay, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetWeekDayViewAllPagingQuery, ResultDTO<PaginatedResult<WeekDayViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<WeekDayViewDTO>>> Handle(
        GetWeekDayViewAllPagingQuery request,
        CancellationToken cancellationToken)
    {
        var query = WeekDayViewRepository
            .MultipleResultQuery()
            .OrderByDescending(x => x.CreateAt);

        var entities = await WeekDayViewRepository.SearchAsync(query, cancellationToken);
        var mappedEntities = entities.Select(x => _mapper.Map<WeekDayViewDTO>(x));

        var paginated = new PaginatedResult<WeekDayViewDTO>(request.PageNumber, request.PageSize);
        await paginated.PaginateFromEnumerable(mappedEntities);

        return ResultDTO<PaginatedResult<WeekDayViewDTO>>.Success(paginated);
    }
}


