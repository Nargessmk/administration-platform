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

public sealed class GetWeekDayRecursiveByIdQueryHandler(
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
        IRequestHandler<GetWeekDayRecursiveByIdQuery, ResultDTO<ConstantRecursiveDTO>>
{
    public async Task<ResultDTO<ConstantRecursiveDTO>> Handle(
        GetWeekDayRecursiveByIdQuery request,
        CancellationToken cancellationToken)
    {
        var _dbContext = _unitOfWork.DbContext as SSSBDBContext
            ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

        var node = await _dbContext
            .GetWeekDayRecursive(request.RootId)
            .Where(x => x.Id == request.ChildId)
            .FirstOrDefaultAsync(cancellationToken);

        if (node is null)
        {
            return ResultDTO<ConstantRecursiveDTO>.Failure(
                "گره در ساختار روز هفته یافت نشد",
                []
            );
        }

        var dto = _mapper.Map<ConstantRecursiveDTO>(node);
        return ResultDTO<ConstantRecursiveDTO>.Success(
            dto,
            "گره سلسله مراتبی روز هفته با موفقیت دریافت شد"
        );
    }
}


