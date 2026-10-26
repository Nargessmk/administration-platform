using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestHeader.View;

public sealed class GetReservationRequestHeaderViewCountTodayQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestHeader> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ReservationRequestHeaderView> ReservationRequestHeaderViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ReservationRequestHeader, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetReservationRequestHeaderViewCountTodayQuery, ResultDTO<int>>
{
    public async Task<ResultDTO<int>> Handle(
        GetReservationRequestHeaderViewCountTodayQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var today = DateTime.Now.Date;
            var startOfDay = today;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            var query = ReservationRequestHeaderViewRepository.MultipleResultQuery()
                .AndFilter(x => x.CreateAt >= startOfDay && x.CreateAt <= endOfDay);

            var entities = await ReservationRequestHeaderViewRepository.SearchAsync(query, cancellationToken);
            var count = entities.Count();

            return ResultDTO<int>.Success(
                count,
                $"تعداد درخواست‌های رزرو ایجاد شده امروز: {count}"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<int>.Failure(
                $"خطا در دریافت تعداد درخواست‌های رزرو: {ex.Message}",
                0
            );
        }
    }
}

