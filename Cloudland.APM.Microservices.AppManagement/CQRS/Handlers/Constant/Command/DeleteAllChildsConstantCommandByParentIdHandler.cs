using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Commands;
using Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.APM.Microservices.AppManagement.Extensions;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Constant.Command;

public sealed class DeleteAllChildsConstantCommandByParentIdHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Cloudland.APM.Models.Constant> repository,
    IMapper map,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Cloudland.APM.Models.Constant, Guid>(unitOfWork, repository, map, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteAllChildsConstantCommandByParentId, int>
{
    public async Task<int> Handle(DeleteAllChildsConstantCommandByParentId request, CancellationToken cancellationToken)
    {
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
        var founded = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        if (founded is { ParentId: null }) return 0;
        
        var childrenQuery = _repository.MultipleResultQuery().AndFilter(item => item.ParentId == request.Id)
            .OrderByDescending(x => x.CreateAt);
        var foundedChildren = await _repository.SearchAsync(childrenQuery, cancellationToken);

        if (foundedChildren.Count > 0)
            return 0;
        
        var effectedRow = await _repository.RemoveAsync(x => x.ParentId == request.Id, cancellationToken);
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