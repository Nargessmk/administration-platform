using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.WeekDay.View;

public sealed class GetWeekDayViewByIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.WeekDay> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<WeekDayView> WeekDayViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.WeekDay, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetWeekDayViewByIdQuery, ResultDTO<WeekDayViewDTO>>
{
    public async Task<ResultDTO<WeekDayViewDTO>> Handle(
        GetWeekDayViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        var query = WeekDayViewRepository
            .SingleResultQuery()
            .AndFilter(x => x.Id == request.Id);

        var row = await WeekDayViewRepository.SingleOrDefaultAsync(query, cancellationToken);
        if (row == null)
        {
            throw new InvalidOperationException("رکورد تقویم یافت نشد");
        }
        var mappedEntity = _mapper.Map<WeekDayViewDTO>(row);
        
        return ResultDTO<WeekDayViewDTO>.Success(mappedEntity);
    }
}


