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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Food.Command;

public sealed class DeleteFoodCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Food> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<DeleteFoodCommandHandler> logger)
    : BaseHandler<Models.Food, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteFoodCommand, ResultDTO<int>?>
{


    public async Task<ResultDTO<int>?> Handle(DeleteFoodCommand request, CancellationToken cancellationToken)
    {
            // Check if entity exists
            var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
            var foodToDelete = await _repository.SingleOrDefaultAsync(query, cancellationToken);
            if (foodToDelete == null)
            {
                return ResultDTO<int>.Failure(
                    $"غذا با این شناسه برای حذف پیدا نشد",
                    0
                ); // No rows affected
            }
            
            if (!foodToDelete.AllowDelete)
            {
                return ResultDTO<int>.Failure(
                    "غذا قابل حذف نمی باشد",
                    null
                ); // No rows affected
            }
        
            if (foodToDelete.Disabled)
            {
                logger.LogInformation($"Food with ID {request.Id} is disabled, proceeding with deletion");
            }

            var affectedRows = await _repository.RemoveAsync(x => x.Id == request.Id, cancellationToken);
        
            if (affectedRows > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
                // Remove from Elasticsearch
                try
                {
                    await _elasticClient.DeleteAsync<FoodView>(request.Id, d => d
                        .Index(ElasticsearchIndexNames.Food)
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
            
                $"غذا {foodToDelete.title} با موفقیت حذف شد"
            );

    }
}
