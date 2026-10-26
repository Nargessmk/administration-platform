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

public sealed class DeleteRoomCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Room> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<DeleteRoomCommandHandler> logger)
    : BaseHandler<Models.Room, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteRoomCommand, ResultDTO<int>?>
{


    public async Task<ResultDTO<int>?> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
            // Check if entity exists
            var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
            var roomToDelete = await _repository.SingleOrDefaultAsync(query, cancellationToken);
            if (roomToDelete == null)
            {
                return ResultDTO<int>.Failure(
                    $"اتاق با این شناسه برای حذف پیدا نشد",
                    0
                ); // No rows affected
            }
            
            if (!roomToDelete.AllowDelete)
            {
                return ResultDTO<int>.Failure(
                    "اتاق قابل حذف نمی باشد",
                    null
                ); // No rows affected
            }
        
            if (roomToDelete.Disabled)
            {
                logger.LogInformation($"Room with ID {request.Id} is disabled, proceeding with deletion");
            }

            var affectedRows = await _repository.RemoveAsync(x => x.Id == request.Id, cancellationToken);
        
            if (affectedRows > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
            }
            else
            {
                logger.LogWarning($"No rows were affected when deleting Room with ID {request.Id}");
            }

            return ResultDTO<int>.Success(
                affectedRows,
            
                $"اتاق {roomToDelete.title} با موفقیت حذف شد"
            );

    }
}
