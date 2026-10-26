using AutoMapper;
using Cloudland.SSSB.DTO;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Models;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;
using Cloudland.SSSB.Microservices.AppManagement.Extensions;
using Cloudland.SSSB.Models.View;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Organizational_Charts.Command;

public sealed class DeleteOrganizationalChartCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<OrganizationalChart> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<DeleteOrganizationalChartCommandHandler> logger)
    : BaseHandler<OrganizationalChart, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteOrganizationalChartCommand, ResultDTO<int>?>
{


    public async Task<ResultDTO<int>?> Handle(DeleteOrganizationalChartCommand request, CancellationToken cancellationToken)
    {

            // Check if entity exists
            var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
            var organizationToDelete = await _repository.SingleOrDefaultAsync(query, cancellationToken);
            if (organizationToDelete == null)
            {
                return ResultDTO<int>.Failure(
                    $"سازمانی با این شناسه برای حذف پیدا نشد",
                    0
                ); // No rows affected
            }
            
            if (!organizationToDelete.AllowDelete)
            {
                return ResultDTO<int>.Failure(
                    "سازمان قابل حذف نمی باشد",
                    null
                ); // No rows affected
            }
        
            if (organizationToDelete.Disabled)
            {
                logger.LogInformation($"OrganizationalChart with ID {request.Id} is disabled, proceeding with deletion");
            }

            var affectedRows = await _repository.RemoveAsync(x => x.Id == request.Id, cancellationToken);
        
            if (affectedRows > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
                // Remove from Elasticsearch
                try
                {
                    await _elasticClient.DeleteAsync<OrganizationalChartView>(request.Id, d => d
                        .Index(ElasticsearchIndexNames.OrganizationalChart)
                    , cancellationToken);
                }
                catch
                {
                    // ignore ES sync errors for now; logging can be added later
                }
            }
            else
            {
                logger.LogWarning($"No rows were affected when deleting OrganizationalChart with ID {request.Id}");
            }

            return ResultDTO<int>.Success(
                affectedRows,
            
                $"شرکت {organizationToDelete.title} با موفقیت حذف شد"
            );

    }
}
