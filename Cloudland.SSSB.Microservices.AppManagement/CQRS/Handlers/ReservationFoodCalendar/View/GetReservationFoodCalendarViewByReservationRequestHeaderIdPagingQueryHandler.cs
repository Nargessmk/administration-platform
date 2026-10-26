using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationFoodCalendar.View;

public sealed class GetReservationFoodCalendarViewByReservationRequestHeaderIdPagingQueryHandler(
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
        IRequestHandler<GetReservationFoodCalendarViewByReservationRequestHeaderIdPagingQuery, ResultDTO<PaginatedResult<ReservationFoodCalendarViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<ReservationFoodCalendarViewDTO>>> Handle(
        GetReservationFoodCalendarViewByReservationRequestHeaderIdPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = reservationFoodCalendarViewRepository.MultipleResultQuery()
                .AndFilter(item => item.ReservationRequestHeaderId == request.ReservationRequestId);

            var entities = await reservationFoodCalendarViewRepository.SearchAsync(query, cancellationToken);

            var paginatedResult = new PaginatedResult<ReservationFoodCalendarViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(entities.Select(x => _mapper.Map<ReservationFoodCalendarViewDTO>(x)));

            return ResultDTO<PaginatedResult<ReservationFoodCalendarViewDTO>>.Success(
                paginatedResult,
                "لیست صفحه‌بندی شده تقویم غذای رزرو بر اساس شناسه هدر رزرو با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ReservationFoodCalendarViewDTO>>.Failure(
                $"خطا در دریافت لیست صفحه‌بندی شده تقویم غذای رزرو بر اساس شناسه هدر رزرو: {ex.Message}",
                []
            );
        }
    }
}
