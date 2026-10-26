using AutoMapper;
using Cloudland.SSSB.DTO;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;
using Cloudland.SSSB.Microservices.AppManagement.Extensions;
using Cloudland.SSSB.Models.View;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Personnel.Command;

public sealed class DeletePersonnelCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Personnel> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<DeletePersonnelCommandHandler> logger)
    : BaseHandler<Models.Personnel, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeletePersonnelCommand, ResultDTO<int>?>
{


    public async Task<ResultDTO<int>?> Handle(DeletePersonnelCommand request, CancellationToken cancellationToken)
    {
            // Check if entity exists
            var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
            var personnelToDelete = await _repository.SingleOrDefaultAsync(query, cancellationToken);
            if (personnelToDelete == null)
            {
                return ResultDTO<int>.Failure(
                    $"پرسنل با این شناسه برای حذف پیدا نشد",
                    0
                ); // No rows affected
            }
            
            if (!personnelToDelete.AllowDelete)
            {
                return ResultDTO<int>.Failure(
                    "پرسنل قابل حذف نمی باشد",
                    null
                ); // No rows affected
            }
        
            if (personnelToDelete.Disabled)
            {
                logger.LogInformation($"Personnel with ID {request.Id} is disabled, proceeding with deletion");
            }

            var affectedRows = await _repository.RemoveAsync(x => x.Id == request.Id, cancellationToken);
        
            if (affectedRows > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
                // Remove from Elasticsearch
                try
                {
                    await _elasticClient.DeleteAsync<PersonnelView>(request.Id, d => d
                        .Index(ElasticsearchIndexNames.Personnel)
                    , cancellationToken);
                }
                catch
                {
                    // ignore ES sync errors for now; logging can be added later
                }
            }
            else
            {
                logger.LogWarning($"No rows were affected when deleting Personnel with ID {request.Id}");
            }

            return ResultDTO<int>.Success(
                affectedRows,
            
                $"پرسنل {personnelToDelete.title} با موفقیت حذف شد"
            );

    }
}
