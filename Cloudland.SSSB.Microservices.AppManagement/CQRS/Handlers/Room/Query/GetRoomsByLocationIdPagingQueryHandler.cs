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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Room.Query;

public sealed class GetRoomsByLocationIdPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Room> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Room, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetRoomsByLocationIdPagingQuery, ResultDTO<PaginatedResult<RoomDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<RoomDTO>>> Handle(GetRoomsByLocationIdPagingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _repository
                .MultipleResultQuery()
                .AndFilter(x => x.LocationId == request.LocationId)
                .AndFilter(x => !x.Disabled && !x.hidden)
                .OrderBy(x => x.title);

            var entities = await _repository.SearchAsync(query, cancellationToken);
            var mappedEntities = entities.Select(x => _mapper.Map<RoomDTO>(x));

            var paginatedResult = new PaginatedResult<RoomDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(mappedEntities);

            return ResultDTO<PaginatedResult<RoomDTO>>.Success(
                paginatedResult,
                $"لیست اتاق‌های مکان با شناسه {request.LocationId} با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<RoomDTO>>.Failure(
                $"خطا در دریافت لیست اتاق‌های مکان: {ex.Message}",
                []
            );
        }
    }
}
