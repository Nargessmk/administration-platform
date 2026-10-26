using AutoMapper;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Contract.Query;

public sealed class GetContractRecursiveHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractRecursive> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractRecursive, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractRecursive, IEnumerable<ContractRecursiveDTO>>
{
    public async Task<IEnumerable<ContractRecursiveDTO>> Handle(GetContractRecursive request, CancellationToken cancellationToken)
    {
        if (_unitOfWork.DbContext is not SSSBDBContext sssbDbContext)
            throw new InvalidOperationException("DbContext is not of the expected type SSSBDBContext.");

        var result = await sssbDbContext.GetContractRecursive()
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ContractRecursiveDTO>>(result);
    }
}


