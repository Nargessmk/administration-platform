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

public sealed class GetRoomByIdQueryHandler(
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
        IRequestHandler<GetRoomByIdQuery, ResultDTO<RoomDTO>>
{
    public async Task<ResultDTO<RoomDTO>> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _repository.SingleResultQuery()
                .AndFilter(item => item.Id == request.Id)
                .AndFilter(item => !item.Disabled)
                .AndFilter(item => !item.hidden);
            
            var room = await _repository.SingleOrDefaultAsync(query, cancellationToken);

            if (room == null)
            {
                return ResultDTO<RoomDTO>.Failure(
                    "اتاق مورد نظر یافت نشد ",
                    []
                );
            }

            return ResultDTO<RoomDTO>.Success(
                _mapper.Map<RoomDTO>(room),
                "اتاق با موفقیت برگشت داده شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<RoomDTO>.Failure(
                $"خطا در دریافت اتاق: {ex.Message}",
                []
            );
        }
    }
}
