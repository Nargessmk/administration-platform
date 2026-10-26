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

public sealed class GetReservationFoodCalendarViewByIdQueryHandler(
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
        IRequestHandler<GetReservationFoodCalendarViewByIdQuery, ResultDTO<ReservationFoodCalendarViewDTO>>
{
    public async Task<ResultDTO<ReservationFoodCalendarViewDTO>> Handle(
        GetReservationFoodCalendarViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = reservationFoodCalendarViewRepository.MultipleResultQuery()
                .AndFilter(item => item.Id == request.Id);

            var entities = await reservationFoodCalendarViewRepository.SearchAsync(query, cancellationToken);
            var entity = entities.FirstOrDefault();

            if (entity == null)
            {
                return ResultDTO<ReservationFoodCalendarViewDTO>.Failure(
                    "تقویم غذای رزرو مورد نظر یافت نشد",
                    new ReservationFoodCalendarViewDTO()
                );
            }

            var dto = _mapper.Map<ReservationFoodCalendarViewDTO>(entity);

            return ResultDTO<ReservationFoodCalendarViewDTO>.Success(
                dto,
                "تقویم غذای رزرو مورد نظر با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<ReservationFoodCalendarViewDTO>.Failure(
                $"خطا در دریافت تقویم غذای رزرو: {ex.Message}",
                new ReservationFoodCalendarViewDTO()
            );
        }
    }
}
