using AutoMapper;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Commands;
using Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.APM.Microservices.AppManagement.Extensions;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Constant.Command;

public sealed class DeleteConstantCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Cloudland.APM.Models.Constant> repository,
    IMapper map,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    ILogger<DeleteConstantCommandHandler> logger,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Cloudland.APM.Models.Constant, Guid>(unitOfWork, repository, map, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteConstantCommand, int>
{
    public async Task<int> Handle(DeleteConstantCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Enter in {nameof(DeleteConstantCommandHandler)}");
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
        var founded = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        if (founded is { ParentId: null }) throw new Exception("Node can not be deleted");

        var effectedRow = await _repository.RemoveAsync(x => x.Id == request.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch - Delete from index
        try
        {
            await _elasticClient.DeleteAsync<ConstantDTO>(request.Id, i => i
                .Index(ElasticsearchIndexNames.Constant)
            , cancellationToken);
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }

        return effectedRow;
    }
}