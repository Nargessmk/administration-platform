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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationFoodCalendar.View;

public sealed class GetReservationFoodCalendarViewAllQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationFoodCalendar> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ReservationFoodCalendarView> reservationFoodCalendarViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ReservationFoodCalendar, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
        distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetReservationFoodCalendarViewAllQuery, ResultDTO<List<ReservationFoodCalendarViewDTO>>>
{
    public async Task<ResultDTO<List<ReservationFoodCalendarViewDTO>>> Handle(
        GetReservationFoodCalendarViewAllQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = reservationFoodCalendarViewRepository.MultipleResultQuery();
            var entities = await reservationFoodCalendarViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<ReservationFoodCalendarViewDTO>>(entities);

            return ResultDTO<List<ReservationFoodCalendarViewDTO>>.Success(
                dtos,
                "لیست کامل تقویم غذای رزرو با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<ReservationFoodCalendarViewDTO>>.Failure(
                $"خطا در دریافت لیست تقویم غذای رزرو: {ex.Message}",
                new List<ReservationFoodCalendarViewDTO>()
            );
        }
    }
}
