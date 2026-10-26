using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Room.Query;

public sealed class GetRoomsByLocationIdQueryHandler(
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
        IRequestHandler<GetRoomsByLocationIdQuery, ResultDTO<List<RoomDTO>>>
{
    public async Task<ResultDTO<List<RoomDTO>>> Handle(GetRoomsByLocationIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _repository
                .MultipleResultQuery()
                .AndFilter(x => x.LocationId == request.LocationId)
                .AndFilter(x => !x.Disabled)
                .AndFilter(x => !x.hidden)
                .OrderBy(x => x.title);

            var rooms = await _repository.SearchAsync(query, cancellationToken);
            var roomDtos = rooms.Select(x => _mapper.Map<RoomDTO>(x)).ToList();

            return ResultDTO<List<RoomDTO>>.Success(
                roomDtos,
                $"لیست اتاق‌های مکان با شناسه {request.LocationId} با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<RoomDTO>>.Failure(
                $"خطا در دریافت لیست اتاق‌های مکان: {ex.Message}",
                [""]
            );
        }
    }
}
