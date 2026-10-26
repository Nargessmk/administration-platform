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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.OrganizationalPosition.Command;

public sealed class DeleteOrganizationalPositionCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.OrganizationalPosition> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<DeleteOrganizationalPositionCommandHandler> logger)
    : BaseHandler<Models.OrganizationalPosition, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteOrganizationalPositionCommand, ResultDTO<int>?>
{


    public async Task<ResultDTO<int>?> Handle(DeleteOrganizationalPositionCommand request, CancellationToken cancellationToken)
    {
            // Check if entity exists
            var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
            var organizationalPositionToDelete = await _repository.SingleOrDefaultAsync(query, cancellationToken);
            if (organizationalPositionToDelete == null)
            {
                return ResultDTO<int>.Failure(
                    $"سمت سازمانی با این شناسه برای حذف پیدا نشد",
                    0
                ); // No rows affected
            }
            
            if (!organizationalPositionToDelete.AllowDelete)
            {
                return ResultDTO<int>.Failure(
                    "سمت سازمانی قابل حذف نمی باشد",
                    null
                ); // No rows affected
            }
        
            if (organizationalPositionToDelete.Disabled)
            {
                logger.LogInformation($"OrganizationalPosition with ID {request.Id} is disabled, proceeding with deletion");
            }

            var affectedRows = await _repository.RemoveAsync(x => x.Id == request.Id, cancellationToken);
        
            if (affectedRows > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
                // Remove from Elasticsearch
                try
                {
                    await _elasticClient.DeleteAsync<OrganizationalPositionView>(request.Id, d => d
                        .Index(ElasticsearchIndexNames.OrganizationalPosition)
                    , cancellationToken);
                }
                catch
                {
                    // ignore ES sync errors for now; logging can be added later
                }
            }
            else
            {
                logger.LogWarning($"No rows were affected when deleting OrganizationalPosition with ID {request.Id}");
            }

            return ResultDTO<int>.Success(
                affectedRows,
            
                $"سمت سازمانی {organizationalPositionToDelete.title} با موفقیت حذف شد"
            );

    }
}
