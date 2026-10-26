using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Room.Command;

public sealed class UpdateRoomCommandHandler(
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
        IRequestHandler<UpdateRoomCommand, ResultDTO<RoomDTO>?>
{


    public async Task<ResultDTO<RoomDTO>?> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Room.Id);
        var existingRoom = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        
        if (existingRoom == null)
        {
            return ResultDTO<RoomDTO>.Failure(
                "اتاق پیدا نشد",
                []
            ); 
        }
        
        if (!existingRoom.AllowUpdate)
        {
            return ResultDTO<RoomDTO>.Failure(
                "اتاق قابل بروزرسانی نمی باشد",
                request.Room
            ); 
        }

        // Check for duplicate room number 
        if (!string.IsNullOrEmpty(request.Room.Number) && request.Room.Number != existingRoom.Number)
        {
            var duplicateQuery = _repository
                .SingleResultQuery()
                .AndFilter(item => item.Number == request.Room.Number 
                                   && item.LocationId == request.Room.LocationId
                                   && item.Id != existingRoom.Id);
            var duplicate = await _repository.SingleOrDefaultAsync(duplicateQuery, cancellationToken);
            if (duplicate != null) 
            {
                return ResultDTO<RoomDTO>.Failure(
                    "اتاق با این شماره وجود دارد", 
                    _mapper.Map<RoomDTO>(duplicate) // Optionally return the duplicate
                );
            }
        }

        existingRoom.LastModifiedAt = DateTime.Now;
        
        // Get user ID from claims
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                existingRoom.LastModifiedUser = userId;
            }
        }

        existingRoom.title = request.Room.title;
        existingRoom.desc = request.Room.desc;
        existingRoom.Number = request.Room.Number;
        existingRoom.LocationId = request.Room.LocationId;
        existingRoom.RoomStatusId = request.Room.RoomStatusId;
        existingRoom.EmptyBedCount = request.Room.EmptyBedCount;
        existingRoom.RoomTypeId = request.Room.RoomTypeId;
        
        
        
        var updatedRoom = _repository.Update(existingRoom);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        return ResultDTO<RoomDTO>.Success(
            _mapper.Map<RoomDTO>(updatedRoom),
            $"اتاق {updatedRoom.title} با موفقیت بروزرسانی شد"
        );

    }
}
