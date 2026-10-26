using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.APM.DataBaseContext;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.APM.Microservices.AppManagement.CQRS.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Constant.Query;

public sealed class GetConstantByDateQueryHandler(
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
        IRequestHandler<GetConstantByDateQuery, IEnumerable<ConstantRecursiveDTO>>
{
    private readonly IUnitOfWork _unitOfWork1 = unitOfWork;

    public async Task<IEnumerable<ConstantRecursiveDTO>> Handle(GetConstantByDateQuery request,
        CancellationToken cancellationToken)
    {
        var apmDbContext = _unitOfWork1.DbContext as APMDBContext;
        if (apmDbContext == null)
        {
            throw new Exception();
        }
        var result = await apmDbContext.GetConstantsRecursive(request.RootId)
            .Where(x => request.dateTime >= x.ValidStartDate && request.dateTime <= x.ValidEndDate)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<ConstantRecursiveDTO>>(result);
    }
}