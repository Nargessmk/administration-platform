using AutoMapper;
using Cloudland.APM.DTO;
using Cloudland.APM.Models;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationFoodCalendar.Command;

public sealed class BulkCreateFoodReservationCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationFoodCalendar> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestHeader> reservationRequestHeaderRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestPerson> reservationRequestPersonRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFoodCalendar> contractFoodCalendarRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFoodLocation> contractFoodLocationRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractLocation> contractLocationRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestBed> reservationRequestBedRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<APM.Models.Calendar> calendarRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ReservationFoodCalendar, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
        distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<BulkCreateFoodReservationCommand, ResultDTO<int>>
{
    private static readonly Guid DefaultAdmissionStatusId = Guid.Parse("0be76d89-a906-4870-9e8b-a8a0d332635a");

    public async Task<ResultDTO<int>> Handle(BulkCreateFoodReservationCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }

        Guid userId;
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (!Guid.TryParse(userClaim, out userId))
            {
                throw new Exception("Invalid user ID in claims");
            }
        }
        else
        {
            throw new Exception("User ID claim not found");
        }

        if (request.Requests == null || !request.Requests.Any())
        {
            return ResultDTO<int>.Failure("لیست درخواست‌ها خالی است", 0);
        }

        // Group requests by ReservationRequestHeaderId
        var groupedRequests = request.Requests.GroupBy(r => r.ReservationRequestHeaderId);

        int totalCreated = 0;

        foreach (var group in groupedRequests)
        {
            var reservationRequestHeaderId = group.Key;

            // Get the reservation request header
            var reservationRequestHeaderQuery = reservationRequestHeaderRepository
                .SingleResultQuery()
                .AndFilter(r => r.Id == reservationRequestHeaderId && !r.Disabled && !r.hidden);
            var reservationRequestHeader = await reservationRequestHeaderRepository.SingleOrDefaultAsync(reservationRequestHeaderQuery, cancellationToken);

            if (reservationRequestHeader == null)
            {
                continue; // Skip if header not found
            }

            if (reservationRequestHeader.FromDate == null || reservationRequestHeader.ToDate == null || reservationRequestHeader.LocationId == null)
            {
                continue; // Skip if required fields are missing
            }

            var fromDate = reservationRequestHeader.FromDate.Value.Date;
            var toDate = reservationRequestHeader.ToDate.Value.Date;
            var locationId = reservationRequestHeader.LocationId.Value;

            // Validate that there is a defined food program (ContractFoodCalendar) for this location and date range
            var contractLocationQuery = contractLocationRepository
                .MultipleResultQuery()
                .AndFilter(cl => cl.LocationId == locationId && !cl.Disabled && !cl.hidden);
            var contractLocations = await contractLocationRepository.SearchAsync(contractLocationQuery, cancellationToken);

            if (!contractLocations.Any())
            {
                return ResultDTO<int>.Failure("برنامه غذایی برای رستوران مورد نظر ناقص تعریف شده است !", 0);
            }

            var contractLocationIds = contractLocations.Select(cl => cl.Id).ToList();

            var contractFoodLocationQuery = contractFoodLocationRepository
                .MultipleResultQuery()
                .AndFilter(cfl => cfl.ContractLocationId.HasValue
                    && contractLocationIds.Contains(cfl.ContractLocationId.Value)
                    && !cfl.Disabled
                    && !cfl.hidden);
            var contractFoodLocations = await contractFoodLocationRepository.SearchAsync(contractFoodLocationQuery, cancellationToken);

            if (!contractFoodLocations.Any())
            {
                return ResultDTO<int>.Failure("برنامه غذایی برای رستوران مورد نظر ناقص تعریف شده است !", 0);
            }

            var contractFoodLocationIds = contractFoodLocations.Select(cfl => cfl.Id).ToList();

            var availableContractFoodCalendarQuery = contractFoodCalendarRepository
                .MultipleResultQuery()
                .AndFilter(cfc => cfc.Date >= fromDate
                    && cfc.Date <= toDate
                    && cfc.Priority == 1
                    && !cfc.Disabled
                    && !cfc.hidden
                    && cfc.ContractFoodLocationId.HasValue
                    && contractFoodLocationIds.Contains(cfc.ContractFoodLocationId.Value));
            var availableContractFoodCalendars = await contractFoodCalendarRepository.SearchAsync(availableContractFoodCalendarQuery, cancellationToken);

            if (!availableContractFoodCalendars.Any())
            {
                return ResultDTO<int>.Failure("برنامه غذایی برای رستوران مورد نظر ناقص تعریف شده است !", 0);
            }

            // Ensure every day in the requested range has at least one defined program
            var availableDates = availableContractFoodCalendars
                .Select(cfc => cfc.Date.Date)
                .ToHashSet();

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                if (!availableDates.Contains(date))
                {
                    return ResultDTO<int>.Failure("برنامه غذایی برای رستوران مورد نظر ناقص تعریف شده است !", 0);
                }
            }

            // Delete all existing ReservationFoodCalendar records related to this Header
            // Date range: FromDate to ToDate
            var deletedCount = await _repository.RemoveAsync(
                rfc => rfc.ReservationRequestHeaderId == reservationRequestHeaderId
                    && rfc.ReservationDate.HasValue
                    && rfc.ReservationDate.Value.Date >= fromDate
                    && rfc.ReservationDate.Value.Date <= toDate
                    && !rfc.Disabled
                    && !rfc.hidden,
                cancellationToken);

            if (deletedCount > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
            }

            // Get all personnel for this reservation request
            var personnelQuery = reservationRequestPersonRepository
                .MultipleResultQuery()
                .AndFilter(rp => rp.ReservationRequestHeaderId == reservationRequestHeaderId && !rp.Disabled && !rp.hidden);
            var personnelRecords = await reservationRequestPersonRepository.SearchAsync(personnelQuery, cancellationToken);
            var personnelList = personnelRecords
                .Where(rp => rp.PersonnelId.HasValue)
                .Select(rp => rp.PersonnelId!.Value)
                .ToList();

            bool shouldCreateBedReservations = reservationRequestHeader.ShiftChangeDayId.HasValue && personnelList.Any();

            if (shouldCreateBedReservations)
            {
                var removedBeds = await reservationRequestBedRepository.RemoveAsync(
                    bed => bed.ReservationRequestHeaderId == reservationRequestHeaderId
                        && bed.ReservationDate.HasValue
                        && bed.ReservationDate.Value.Date >= fromDate
                        && bed.ReservationDate.Value.Date <= toDate
                        && !bed.Disabled
                        && !bed.hidden,
                    cancellationToken);

                if (removedBeds > 0)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
                }

                var newBedReservations = new List<Models.ReservationRequestBed>();
                for (var date = fromDate; date <= toDate; date = date.AddDays(1))
                {
                    foreach (var personnelId in personnelList)
                    {
                        var bedReservation = new Models.ReservationRequestBed
                        {
                            Id = Guid.NewGuid(),
                            ReservationRequestHeaderId = reservationRequestHeaderId,
                            PersonnelId = personnelId,
                            BedId = null,
                            ReservationDate = date,
                            AdmissionStatus = DefaultAdmissionStatusId,
                            RoomDeliveryStatus = false,
                            DeliveryDate = null,
                            DeliveryTime = null,
                            DocumentReceipt = null,
                            CancellationStatus = false,
                            CreateAt = DateTime.Now,
                            LastModifiedAt = DateTime.Now,
                            UserCreated = userId,
                            LastModifiedUser = userId,
                            CustomerId = reservationRequestHeader.CustomerId,
                            AllowDelete = true,
                            AllowUpdate = true,
                            Disabled = false,
                            hidden = false,
                            title = string.Empty,
                            desc = string.Empty
                        };

                        newBedReservations.Add(bedReservation);
                    }
                }

                foreach (var bedReservation in newBedReservations)
                {
                    await reservationRequestBedRepository.AddAsync(bedReservation, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
            }

            // Get Calendar data using repository pattern
            var calendarQuery = calendarRepository
                .MultipleResultQuery()
                .AndFilter(cal => cal.Date >= fromDate && cal.Date <= toDate && !cal.Disabled && !cal.hidden);
            var calendarRecords = await calendarRepository.SearchAsync(calendarQuery, cancellationToken);
            
            var calendarData = calendarRecords
                .Select(cal => new
                {
                    cal.Date,
                    cal.IsHoliday,
                    cal.IsThursday,
                    cal.IsFriday
                })
                .ToList();

            // Create a dictionary for quick lookup
            var calendarLookup = calendarData.ToDictionary(
                cal => cal.Date.Date,
                cal => new { cal.IsHoliday, cal.IsThursday, cal.IsFriday }
            );

            // Process each request in the group
            foreach (var shiftRequest in group)
            {
                // Query ContractFoodCalendar with filters using repository
                // First, get all ContractFoodCalendars matching date, meal, and priority
                var contractFoodCalendarQuery = contractFoodCalendarRepository
                    .MultipleResultQuery()
                    .AndFilter(cfc => cfc.Date >= fromDate
                        && cfc.Date <= toDate
                        && cfc.MealId == shiftRequest.MealId
                        && cfc.Priority == 1
                        && !cfc.Disabled
                        && !cfc.hidden);
                var allContractFoodCalendars = await contractFoodCalendarRepository.SearchAsync(contractFoodCalendarQuery, cancellationToken);

                // Get all ContractFoodLocations and ContractLocations to filter by LocationId
                var requestContractFoodLocationIds = allContractFoodCalendars
                    .Where(cfc => cfc.ContractFoodLocationId.HasValue)
                    .Select(cfc => cfc.ContractFoodLocationId!.Value)
                    .Distinct()
                    .ToList();

                if (!requestContractFoodLocationIds.Any())
                {
                    continue;
                }

                var requestContractFoodLocationQuery = contractFoodLocationRepository
                    .MultipleResultQuery()
                    .AndFilter(cfl => requestContractFoodLocationIds.Contains(cfl.Id));
                var requestContractFoodLocations = await contractFoodLocationRepository.SearchAsync(requestContractFoodLocationQuery, cancellationToken);

                var requestContractLocationIds = requestContractFoodLocations
                    .Where(cfl => cfl.ContractLocationId.HasValue)
                    .Select(cfl => cfl.ContractLocationId!.Value)
                    .Distinct()
                    .ToList();

                if (!requestContractLocationIds.Any())
                {
                    continue;
                }

                var requestContractLocationQuery = contractLocationRepository
                    .MultipleResultQuery()
                    .AndFilter(cl => requestContractLocationIds.Contains(cl.Id) && cl.LocationId == locationId);
                var matchingContractLocations = await contractLocationRepository.SearchAsync(requestContractLocationQuery, cancellationToken);
                var matchingContractLocationIds = matchingContractLocations.Select(cl => cl.Id).ToHashSet();

                // Filter ContractFoodCalendars by matching LocationId
                var matchingContractFoodLocationIds = requestContractFoodLocations
                    .Where(cfl => cfl.ContractLocationId.HasValue && matchingContractLocationIds.Contains(cfl.ContractLocationId.Value))
                    .Select(cfl => cfl.Id)
                    .ToHashSet();

                var allMatchingCalendars = allContractFoodCalendars
                    .Where(cfc => cfc.ContractFoodLocationId.HasValue && matchingContractFoodLocationIds.Contains(cfc.ContractFoodLocationId.Value))
                    .Select(cfc => new { cfc.Id, cfc.Date })
                    .ToList();

                // Filter by Calendar table data (IsHoliday, IsThursday, IsFriday)
                var matchingCalendars = allMatchingCalendars
                    .Where(cfc =>
                    {
                        var dateKey = cfc.Date.Date;
                        if (!calendarLookup.TryGetValue(dateKey, out var calInfo))
                            return false; // No calendar data for this date

                        // Check IsHoliday match
                        if (calInfo.IsHoliday != shiftRequest.IsHoliday)
                            return false;

                        // Apply IsThursday filter
                        if (shiftRequest.IsThursday && !calInfo.IsThursday)
                            return false;

                        if (!shiftRequest.IsThursday && calInfo.IsThursday)
                            return false;

                        // Exclude Fridays if IsHoliday is false and IsThursday is false
                        if (!shiftRequest.IsHoliday && !shiftRequest.IsThursday && calInfo.IsFriday)
                            return false;

                        return true;
                    })
                    .ToList();

                if (!matchingCalendars.Any())
                {
                    continue; // No matching calendar entries
                }

                // Calculate how many entries to create with and without PersonId
                int personnelCount = personnelList.Count;
                int entriesWithPersonnel = Math.Min(shiftRequest.FoodCount, personnelCount);
                int entriesWithoutPersonnel = Math.Max(0, shiftRequest.FoodCount - personnelCount);

                // Create reservations for each matching calendar day
                foreach (var calendar in matchingCalendars)
                {
                    // Create entries with PersonId (one per personnel, up to FoodCount)
                    for (int i = 0; i < entriesWithPersonnel; i++)
                    {
                        var reservation = new Models.ReservationFoodCalendar
                        {
                            Id = Guid.NewGuid(),
                            ReservationRequestHeaderId = reservationRequestHeaderId,
                            PersonnelId = personnelList[i],
                            ContractFoodCalendarId = calendar.Id,
                            ReservationDate = calendar.Date,
                            Quantity = 1, // Smallest quantity as specified
                            CancelationStatus = false,
                            CreateAt = DateTime.Now,
                            LastModifiedAt = DateTime.Now,
                            UserCreated = userId,
                            LastModifiedUser = userId,
                            CustomerId = Guid.NewGuid(),
                            AllowDelete = true,
                            AllowUpdate = true,
                            Disabled = false,
                            hidden = false,
                            title = " "
                        };

                        await _repository.AddAsync(reservation, cancellationToken);
                        totalCreated++;
                    }

                    // Create remaining entries without PersonId (Quantity = 1 each)
                    for (int i = 0; i < entriesWithoutPersonnel; i++)
                    {
                        var reservation = new Models.ReservationFoodCalendar
                        {
                            Id = Guid.NewGuid(),
                            ReservationRequestHeaderId = reservationRequestHeaderId,
                            PersonnelId = null,
                            ContractFoodCalendarId = calendar.Id,
                            ReservationDate = calendar.Date,
                            Quantity = 1, // Smallest quantity as specified
                            CancelationStatus = false,
                            CreateAt = DateTime.Now,
                            LastModifiedAt = DateTime.Now,
                            UserCreated = userId,
                            LastModifiedUser = userId,
                            CustomerId = Guid.NewGuid(),
                            AllowDelete = true,
                            AllowUpdate = true,
                            Disabled = false,
                            hidden = false,
                            title = " "
                        };

                        await _repository.AddAsync(reservation, cancellationToken);
                        totalCreated++;
                    }
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        return ResultDTO<int>.Success(
            totalCreated,
            $"تعداد {totalCreated} رکورد رزرو غذا با موفقیت ایجاد شد"
        );
    }
}

