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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Bed.Command;

public sealed class DeleteBedCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Bed> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<DeleteBedCommandHandler> logger)
    : BaseHandler<Models.Bed, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteBedCommand, ResultDTO<int>?>
{


    public async Task<ResultDTO<int>?> Handle(DeleteBedCommand request, CancellationToken cancellationToken)
    {
            // Check if entity exists
            var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
            var bedToDelete = await _repository.SingleOrDefaultAsync(query, cancellationToken);
            if (bedToDelete == null)
            {
                return ResultDTO<int>.Failure(
                    $"تخت با این شناسه برای حذف پیدا نشد",
                    0
                ); // No rows affected
            }
            
            if (!bedToDelete.AllowDelete)
            {
                return ResultDTO<int>.Failure(
                    "تخت قابل حذف نمی باشد",
                    null
                ); // No rows affected
            }
        
            if (bedToDelete.Disabled)
            {
                logger.LogInformation($"Bed with ID {request.Id} is disabled, proceeding with deletion");
            }

            var affectedRows = await _repository.RemoveAsync(x => x.Id == request.Id, cancellationToken);
        
            if (affectedRows > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
            }
            else
            {
                logger.LogWarning($"No rows were affected when deleting Bed with ID {request.Id}");
            }

            return ResultDTO<int>.Success(
                affectedRows,
            
                $"تخت {bedToDelete.title} با موفقیت حذف شد"
            );

    }
}
