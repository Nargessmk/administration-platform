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
using Cloudland.SSSB.Microservices.AppManagement.Extensions;
using Cloudland.SSSB.Models.View;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Location.Command;

public sealed class DeleteLocationCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Location> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<DeleteLocationCommandHandler> logger)
    : BaseHandler<Models.Location, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteLocationCommand, ResultDTO<int>?>
{


    public async Task<ResultDTO<int>?> Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
    {
            var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
            var locationToDelete = await _repository.SingleOrDefaultAsync(query, cancellationToken);
            if (locationToDelete == null)
            {
                return ResultDTO<int>.Failure(
                    $"محل با این شناسه برای حذف پیدا نشد",
                    0
                ); 
            }
            
            if (!locationToDelete.AllowDelete)
            {
                return ResultDTO<int>.Failure(
                    "محل قابل حذف نمی باشد",
                    null
                );
            }
        
            if (locationToDelete.Disabled)
            {
                logger.LogInformation($"Location with ID {request.Id} is disabled, proceeding with deletion");
            }

            var affectedRows = await _repository.RemoveAsync(x => x.Id == request.Id, cancellationToken);
        
            if (affectedRows > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
                // Remove from Elasticsearch
                try
                {
                    await _elasticClient.DeleteAsync<LocationView>(request.Id, d => d
                        .Index(ElasticsearchIndexNames.Location)
                    , cancellationToken);
                }
                catch
                {
                    // ignore ES sync errors for now; logging can be added later
                }
            }
            else
            {
                logger.LogWarning($"No rows were affected when deleting Food with ID {request.Id}");
            }

            return ResultDTO<int>.Success(
                affectedRows,
            
                $"محل {locationToDelete.title} با موفقیت حذف شد"
            );

    }
}
