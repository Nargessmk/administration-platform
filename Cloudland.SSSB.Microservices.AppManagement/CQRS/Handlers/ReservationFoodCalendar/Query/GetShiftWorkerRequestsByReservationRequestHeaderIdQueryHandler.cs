using AutoMapper;
using Cloudland.APM.DTO;
using Cloudland.APM.Models;
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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationFoodCalendar.Query;

public sealed class GetShiftWorkerRequestsByReservationRequestHeaderIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationFoodCalendar> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestHeader> reservationRequestHeaderRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<APM.Models.Calendar> calendarRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ReservationFoodCalendar, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
        distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetShiftWorkerRequestsByReservationRequestHeaderIdQuery, ResultDTO<List<ShiftWorkerRequestDTO>>>
{
    public async Task<ResultDTO<List<ShiftWorkerRequestDTO>>> Handle(
        GetShiftWorkerRequestsByReservationRequestHeaderIdQuery request, 
        CancellationToken cancellationToken)
    {
        // Get the reservation request header to get ShiftChangeDayId
        var reservationRequestHeaderQuery = reservationRequestHeaderRepository
            .SingleResultQuery()
            .AndFilter(r => r.Id == request.ReservationRequestHeaderId && !r.Disabled && !r.hidden);
        var reservationRequestHeader = await reservationRequestHeaderRepository.SingleOrDefaultAsync(reservationRequestHeaderQuery, cancellationToken);

        if (reservationRequestHeader == null)
        {
            return ResultDTO<List<ShiftWorkerRequestDTO>>.Failure(
                "درخواست رزرو یافت نشد",
                new List<ShiftWorkerRequestDTO>()
            );
        }

        // Get all reservation food calendars for this header using projection to avoid nullable field issues
        // Use DbContext through unit of work for projection query (still part of repository pattern)
        var dbContext = _unitOfWork.DbContext as SSSBDBContext;
        if (dbContext == null)
        {
            throw new Exception("SSSB database context is not available");
        }

        // Use explicit projection to handle nullable PersonnelId correctly
        var reservations = await (from rfc in dbContext.ReservationFoodCalendars
                                  join cfc in dbContext.ContractFoodCalendars on rfc.ContractFoodCalendarId equals cfc.Id
                                  where rfc.ReservationRequestHeaderId == request.ReservationRequestHeaderId
                                      && !rfc.Disabled
                                      && !rfc.hidden
                                      && !rfc.CancelationStatus
                                  select new
                                  {
                                      rfc.ReservationDate,
                                      rfc.Quantity,
                                      cfc.MealId
                                  })
                                  .ToListAsync(cancellationToken);

        if (!reservations.Any())
        {
            return ResultDTO<List<ShiftWorkerRequestDTO>>.Success(
                new List<ShiftWorkerRequestDTO>(),
                "هیچ رزرو غذایی یافت نشد"
            );
        }

        // Get calendar data for all reservation dates using repository
        var reservationDates = reservations
            .Where(r => r.ReservationDate.HasValue)
            .Select(r => r.ReservationDate!.Value.Date)
            .Distinct()
            .ToList();

        var calendarQuery = calendarRepository
            .MultipleResultQuery()
            .AndFilter(cal => reservationDates.Contains(cal.Date.Date) && !cal.Disabled && !cal.hidden);
        var calendarRecords = await calendarRepository.SearchAsync(calendarQuery, cancellationToken);
        
        var calendarData = calendarRecords
            .Select(cal => new
            {
                cal.Date.Date,
                cal.IsHoliday,
                cal.IsThursday,
                cal.IsFriday
            })
            .ToList();

        var calendarLookup = calendarData.ToDictionary(
            cal => cal.Date,
            cal => new { cal.IsHoliday, cal.IsThursday, cal.IsFriday }
        );

        // Group reservations by MealId, IsHoliday, and IsThursday
        var groupedReservations = reservations
            .Where(r => r.ReservationDate.HasValue && r.MealId != Guid.Empty)
            .GroupBy(r =>
            {
                var dateKey = r.ReservationDate!.Value.Date;
                var calInfo = calendarLookup.GetValueOrDefault(dateKey);
                
                return new
                {
                    MealId = r.MealId,
                    IsHoliday = calInfo?.IsHoliday ?? false,
                    IsThursday = calInfo?.IsThursday ?? false
                };
            })
            .Select(g =>
            {
                var distinctDays = g
                    .Where(r => r.ReservationDate.HasValue)
                    .Select(r => r.ReservationDate!.Value.Date)
                    .Distinct()
                    .Count();

                var totalQuantity = g.Sum(r => r.Quantity);
                var perDayFoodCount = distinctDays > 0
                    ? totalQuantity / distinctDays
                    : totalQuantity;

                return new ShiftWorkerRequestDTO
                {
                    Id = Guid.NewGuid(),
                    ReservationRequestHeaderId = request.ReservationRequestHeaderId,
                    ShiftChangeDay = reservationRequestHeader.ShiftChangeDayId?.ToString() ?? string.Empty,
                    IsHoliday = g.Key.IsHoliday,
                    IsThursday = g.Key.IsThursday,
                    MealId = g.Key.MealId,
                    FoodCount = perDayFoodCount
                };
            })
            .ToList();

        return ResultDTO<List<ShiftWorkerRequestDTO>>.Success(
            groupedReservations,
            $"تعداد {groupedReservations.Count} درخواست شیفت کارگر با موفقیت دریافت شد"
        );
    }
}

