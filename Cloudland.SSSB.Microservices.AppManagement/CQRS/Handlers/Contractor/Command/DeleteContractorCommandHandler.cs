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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Contractor.Command;

public sealed class DeleteContractorCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Contractor> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<DeleteContractorCommandHandler> logger)
    : BaseHandler<Models.Contractor, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteContractorCommand, ResultDTO<int>?>
{


    public async Task<ResultDTO<int>?> Handle(DeleteContractorCommand request, CancellationToken cancellationToken)
    {
            // Check if entity exists
            var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
            var contractorToDelete = await _repository.SingleOrDefaultAsync(query, cancellationToken);
            if (contractorToDelete == null)
            {
                return ResultDTO<int>.Failure(
                    $"پیمانکاری با این شناسه برای حذف پیدا نشد",
                    0
                ); // No rows affected
            }
            
            if (!contractorToDelete.AllowDelete)
            {
                return ResultDTO<int>.Failure(
                    "پیمانکار قابل حذف نمی باشد",
                    null
                ); // No rows affected
            }
        
            if (contractorToDelete.Disabled)
            {
                logger.LogInformation($"Contractor with ID {request.Id} is disabled, proceeding with deletion");
            }

            var affectedRows = await _repository.RemoveAsync(x => x.Id == request.Id, cancellationToken);
        
            if (affectedRows > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
                // Remove from Elasticsearch
                try
                {
                    await _elasticClient.DeleteAsync<ContractorView>(request.Id, d => d
                        .Index(ElasticsearchIndexNames.Contractor)
                    , cancellationToken);
                }
                catch
                {
                    // ignore ES sync errors for now; logging can be added later
                }
            }
            else
            {
                logger.LogWarning($"No rows were affected when deleting Contractor with ID {request.Id}");
            }

            return ResultDTO<int>.Success(
                affectedRows,
            
                $"پیمانکار {contractorToDelete.title} با موفقیت حذف شد"
            );

    }
}
