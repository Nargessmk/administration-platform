using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Room.View;

public sealed class GetRoomViewsByLocationIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Room> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<RoomView> roomViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Room, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetRoomViewsByLocationIdQuery, ResultDTO<List<RoomViewDTO>>>
{
    public async Task<ResultDTO<List<RoomViewDTO>>> Handle(
        GetRoomViewsByLocationIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = roomViewRepository.MultipleResultQuery()
                .AndFilter(item => item.LocationId == request.LocationId);

            var entities = await roomViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<RoomViewDTO>>(entities);

            return ResultDTO<List<RoomViewDTO>>.Success(
                dtos,
                "لیست اتاق‌های مکان مورد نظر با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<RoomViewDTO>>.Failure(
                $"خطا در دریافت لیست اتاق‌های مکان: {ex.Message}",
                new List<RoomViewDTO>()
            );
        }
    }
}
