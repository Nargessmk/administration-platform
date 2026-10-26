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

public sealed class AddRoomCommandHandler(
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
        IRequestHandler<AddRoomCommand, ResultDTO<RoomDTO>?>
{
    public async Task<ResultDTO<RoomDTO>?> Handle(AddRoomCommand request, CancellationToken cancellationToken)
    {

            if (_httpContextAccessor.HttpContext == null)
            {
                throw new Exception("HTTP context is not available");
            }

            // Check for duplicate title
            var queryDuplicate = _repository
                .SingleResultQuery()
                .AndFilter(item => item.Number == request.Room.Number && item.LocationId == request.Room.LocationId);
            var duplicate = await _repository.SingleOrDefaultAsync(queryDuplicate, cancellationToken);
            if (duplicate != null)
            {
                return ResultDTO<RoomDTO>.Failure(
                    "اتاقی با این شماره برای این رستوران از قبل موجود می باشد",
                    _mapper.Map<RoomDTO>(duplicate)
                );
            }

            // Map DTO to entity
            var newRoom = _mapper.Map<Models.Room>(request.Room);

            newRoom.CreateAt = DateTime.Now;
            newRoom.LastModifiedAt = DateTime.Now;
        

            if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
            {
                var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
                if (Guid.TryParse(userClaim, out Guid userId))
                {
                    newRoom.UserCreated = userId;
                    newRoom.LastModifiedUser = userId;
                }
                else
                {
                    throw new Exception("Invalid user ID in claims");
                }
            }
            else
            {
                throw new Exception("User ID claim not found");
            }

            newRoom.CustomerId = Guid.NewGuid();
            // Set default values
            newRoom.AllowDelete = true;
            newRoom.AllowUpdate = true;
            newRoom.Disabled = false;
            newRoom.hidden = false;

            // Add organizational chart to repository
            var room = await _repository.AddAsync(newRoom, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

            return ResultDTO<RoomDTO>.Success(
                _mapper.Map<RoomDTO>(room),
                "اتاق با موفقیت ایجاد شد"
            );
    }
}
