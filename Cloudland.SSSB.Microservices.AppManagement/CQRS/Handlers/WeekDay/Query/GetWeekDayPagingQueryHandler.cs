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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.WeekDay.Query;

public sealed class GetWeekDayPagingQueryHandler(
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
        IRequestHandler<GetWeekDayPagingQuery, ResultDTO<PaginatedResult<WeekDayDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<WeekDayDTO>>> Handle(GetWeekDayPagingQuery request, CancellationToken cancellationToken)
    {
        
        try
        {
            var query = _repository
                .MultipleResultQuery()
                .AndFilter( x => !x.Disabled && !x.hidden)
                .OrderBy(x => x.CreateAt);

            var entities = await _repository.SearchAsync(query, cancellationToken);
            var mappedEntities = entities.Select(x => _mapper.Map<WeekDayDTO>(x));

            var paginatedResult = new PaginatedResult<WeekDayDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(mappedEntities);

            return ResultDTO<PaginatedResult<WeekDayDTO>>.Success(
                paginatedResult,
                "لیست روز های هفته با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<WeekDayDTO>>.Failure(
                $"خطا در دریافت لیست روز های هفته: {ex.Message}",
                []
            );
        }

    }
}


