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

public sealed class GetRootConstantQueryHandler(
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
        IRequestHandler<GetRootConstantQuery, IEnumerable<ConstantDTO>>
{
    public async Task<IEnumerable<ConstantDTO>> Handle(GetRootConstantQuery request,
        CancellationToken cancellationToken)
    {
        var query = _repository.MultipleResultQuery().AndFilter(item => item.ParentId == null)
            .OrderBy(x => x.Priority);
        return _mapper.Map<IEnumerable<ConstantDTO>>(await _repository.SearchAsync(query, cancellationToken));
    }
}