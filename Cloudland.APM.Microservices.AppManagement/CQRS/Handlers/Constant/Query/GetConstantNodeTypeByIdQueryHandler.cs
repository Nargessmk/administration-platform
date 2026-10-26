using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.APM.Microservices.AppManagement.CQRS.Queries;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Constant.Query;

public sealed class GetConstantNodeTypeByIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Cloudland.APM.Models.Constant> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Cloudland.APM.Models.Constant, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetConstantNodeTypeByIdQuery, List<ConstantDTO>>
{
    private static readonly Guid SpecialNodeTypeId = new Guid("ec640c73-0bda-42ee-b7f7-f946ea3354dd");
    public async Task<List<ConstantDTO>> Handle(GetConstantNodeTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.NodeTypeId == SpecialNodeTypeId)
        {
            
            var currentNodeTypeId = request.NodeTypeId;
            Guid? constantIdToSearch = request.Id;
            
            
            while (currentNodeTypeId == SpecialNodeTypeId)
            {
                var constantQuery = _repository.SingleResultQuery()
                    .AndFilter(x => x.Id == constantIdToSearch);
                var constant = await _repository.FirstOrDefaultAsync(constantQuery, cancellationToken);
                
                var parentQuery = _repository.SingleResultQuery()
                    .AndFilter(x => x.Id == constant.ParentId);
                var parent = await _repository.FirstOrDefaultAsync(parentQuery, cancellationToken);

                if (parent == null)
                    break;
                
                constantIdToSearch = parent.Id;
                currentNodeTypeId = parent.NodeTypeId ?? Guid.Empty;
            }

            if (currentNodeTypeId.HasValue)
            {
                var childsQuery = _repository.MultipleResultQuery()
                    .AndFilter(item => item.ParentId == currentNodeTypeId || item.Id == currentNodeTypeId)
                    .OrderBy(x => x.Priority);

                var children = await _repository.SearchAsync(childsQuery, cancellationToken);
                return _mapper.Map<List<ConstantDTO>>(children);
            }
            
            return new List<ConstantDTO>();
        }
        else if(request.NodeTypeId != null)
        {
            var childsQuery = _repository.MultipleResultQuery()
                .AndFilter(item => item.ParentId == request.NodeTypeId || item.Id == request.NodeTypeId)
                .OrderBy(x => x.Priority);
            var children = await _repository.SearchAsync(childsQuery, cancellationToken);

            return _mapper.Map<List<ConstantDTO>>(children);
        }
        else
        {
            var nodesQuery = _repository.SingleResultQuery()
                .AndFilter(item => item.Id == request.Id)
                .OrderBy(x => x.Priority);
            var node = await _repository.SingleOrDefaultAsync(nodesQuery, cancellationToken);
            node.title = "افزودن جدید";
            
            return _mapper.Map<List<ConstantDTO>>(node);
        }
    }
}