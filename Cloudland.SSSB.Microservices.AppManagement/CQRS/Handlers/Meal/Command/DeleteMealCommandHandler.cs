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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Meal.Command;

public sealed class DeleteMealCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Meal> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<DeleteMealCommandHandler> logger)
    : BaseHandler<Models.Meal, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteMealCommand, ResultDTO<int>?>
{


    public async Task<ResultDTO<int>?> Handle(DeleteMealCommand request, CancellationToken cancellationToken)
    {
            // Check if entity exists
            var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
            var mealToDelete = await _repository.SingleOrDefaultAsync(query, cancellationToken);
            if (mealToDelete == null)
            {
                return ResultDTO<int>.Failure(
                    $"وعده غذایی با این شناسه برای حذف پیدا نشد",
                    0
                ); // No rows affected
            }
            
            if (!mealToDelete.AllowDelete)
            {
                return ResultDTO<int>.Failure(
                    "وعده غذایی قابل حذف نمی باشد",
                    null
                ); // No rows affected
            }
        
            if (mealToDelete.Disabled)
            {
                logger.LogInformation($"Meal with ID {request.Id} is disabled, proceeding with deletion");
            }

            var affectedRows = await _repository.RemoveAsync(x => x.Id == request.Id, cancellationToken);
        
            if (affectedRows > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
                // Remove from Elasticsearch
                try
                {
                    await _elasticClient.DeleteAsync<MealView>(request.Id, d => d
                        .Index(ElasticsearchIndexNames.Meal)
                    , cancellationToken);
                }
                catch
                {
                    // ignore ES sync errors for now; logging can be added later
                }
            }
            else
            {
                logger.LogWarning($"No rows were affected when deleting Meal with ID {request.Id}");
            }

            return ResultDTO<int>.Success(
                affectedRows,
            
                $"وعده غذایی {mealToDelete.title} با موفقیت حذف شد"
            );

    }
}
