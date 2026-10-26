using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.APM.DataBaseContext;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.APM.Microservices.AppManagement.CQRS.Queries;
using Cloudland.APM.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Constant.Query;

public sealed class GetConstantRecursiveByRootIdHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ConstantRecursive> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<ConstantRecursive, Guid>(unitOfWork, repository, mapper, publishEndpoint,
            elasticClient, distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetConstantRecursiveByRootId, List<ConstantRecursiveDTO>>
{
    public async Task<List<ConstantRecursiveDTO>> Handle(GetConstantRecursiveByRootId request,
        CancellationToken cancellationToken)
    {
        if (_unitOfWork.DbContext is not APMDBContext apmDbContext)
            throw new InvalidOperationException("DbContext is not of the expected type APMDBContext.");

        var result = await apmDbContext.GetConstantsRecursive(request.RootId)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<ConstantRecursiveDTO>>(result);
    }
}