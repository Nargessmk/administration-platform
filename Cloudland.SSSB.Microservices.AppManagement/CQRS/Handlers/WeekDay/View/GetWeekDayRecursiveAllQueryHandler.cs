using AutoMapper;
using Cloudland.APM.DTO;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.WeekDay.View;

public sealed class GetWeekDayRecursiveAllQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.WeekDay> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.WeekDay, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetWeekDayRecursiveAllQuery, ResultDTO<List<ConstantRecursiveDTO>>>
{
    public async Task<ResultDTO<List<ConstantRecursiveDTO>>> Handle(
        GetWeekDayRecursiveAllQuery request,
        CancellationToken cancellationToken)
    {
        var _dbContext = _unitOfWork.DbContext as SSSBDBContext
            ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

        var list = await _dbContext
            .GetWeekDayRecursive(request.RootId)
            .Select(x => _mapper.Map<ConstantRecursiveDTO>(x))
            .ToListAsync(cancellationToken);

        return ResultDTO<List<ConstantRecursiveDTO>>.Success(
            list,
            "ساختار روز های هفته با موفقیت دریافت شد"
        );
    }
}


