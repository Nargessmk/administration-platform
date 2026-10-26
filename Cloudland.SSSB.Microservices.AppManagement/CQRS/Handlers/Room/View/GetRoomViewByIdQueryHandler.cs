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

public sealed class GetRoomViewByIdQueryHandler(
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
        IRequestHandler<GetRoomViewByIdQuery, ResultDTO<RoomViewDTO>>
{
    public async Task<ResultDTO<RoomViewDTO>> Handle(
        GetRoomViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = roomViewRepository.SingleResultQuery()
                .AndFilter(item => item.Id == request.Id);

            var entity = await roomViewRepository.SingleOrDefaultAsync(query, cancellationToken);

            if (entity == null)
            {
                return ResultDTO<RoomViewDTO>.Failure(
                    "اتاق مورد نظر یافت نشد",
                    []
                );
            }

            var dto = _mapper.Map<RoomViewDTO>(entity);

            return ResultDTO<RoomViewDTO>.Success(
                dto,
                "اتاق با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<RoomViewDTO>.Failure(
                $"خطا در دریافت اتاق: {ex.Message}",
                []
            );
        }
    }
}
