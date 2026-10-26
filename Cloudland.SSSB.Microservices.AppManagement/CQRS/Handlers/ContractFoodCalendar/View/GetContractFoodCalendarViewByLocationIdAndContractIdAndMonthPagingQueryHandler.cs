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
using MD.PersianDateTime.Standard;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFoodCalendar.View;

public sealed class GetContractFoodCalendarViewByLocationIdAndContractIdAndMonthPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFoodCalendar> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ContractFoodCalendarView> contractFoodCalendarViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractFoodCalendar, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractFoodCalendarViewByLocationIdAndContractIdAndMonthPagingQuery, ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>>
{
    private static readonly Dictionary<string, int> PersianMonthMap = new()
    {
        { "فروردین", 1 }, { "اردیبهشت", 2 }, { "خرداد", 3 }, { "تیر", 4 },
        { "مرداد", 5 }, { "شهریور", 6 }, { "مهر", 7 }, { "آبان", 8 },
        { "آذر", 9 }, { "دی", 10 }, { "بهمن", 11 }, { "اسفند", 12 }
    };

    public async Task<ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>> Handle(
        GetContractFoodCalendarViewByLocationIdAndContractIdAndMonthPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Parse Persian month and get date range in Gregorian calendar
            var (startDate, endDate) = GetPersianMonthDateRange(request.Month);

            var query = contractFoodCalendarViewRepository
                .MultipleResultQuery()
                .AndFilter(item => item.LocationId == request.LocationId)
                .AndFilter(item => item.ContractId == request.ContractId)
                .AndFilter(item => item.Date >= startDate && item.Date <= endDate)
                .OrderBy(item => item.Date.Day);

            var entities = await contractFoodCalendarViewRepository.SearchAsync(query, cancellationToken);

            var paginatedResult = new PaginatedResult<ContractFoodCalendarViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(entities.Select(x => _mapper.Map<ContractFoodCalendarViewDTO>(x)));

            return ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>.Success(
                paginatedResult,
                "لیست صفحه‌بندی شده تقویم وعده‌ها بر اساس مکان، قرارداد و ماه با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>.Failure(
                $"خطا در دریافت لیست صفحه‌بندی شده تقویم وعده‌ها بر اساس مکان، قرارداد و ماه: {ex.Message}",
                []
            );
        }
    }

    private static (DateTime startDate, DateTime endDate) GetPersianMonthDateRange(string month)
    {
        if (string.IsNullOrWhiteSpace(month)) throw new ArgumentException("Month is required");
        month = month.Trim();

        int persianMonthNumber;

        if (int.TryParse(month, out var num))
        {
            if (num >= 1 && num <= 12) 
                persianMonthNumber = num;
            else if (month.Length == 2 && month.StartsWith('0') && int.TryParse(month[1].ToString(), out var num2) && num2 >= 1 && num2 <= 9)
                persianMonthNumber = num2;
            else
                throw new ArgumentException("فرمت ماه نامعتبر است. مقدار باید 01..12 یا نام فارسی ماه باشد");
        }
        else if (PersianMonthMap.TryGetValue(month, out var pnum))
        {
            persianMonthNumber = pnum;
        }
        else
        {
            throw new ArgumentException("فرمت ماه نامعتبر است. مقدار باید 01..12 یا نام فارسی ماه باشد");
        }
        
        var currentPersianYear = new PersianDateTime(DateTime.Now).Year;

        var persianStartDate = new PersianDateTime(currentPersianYear, persianMonthNumber, 1);

        var nextMonth = persianMonthNumber == 12 ? 1 : persianMonthNumber + 1;
        var nextYear = persianMonthNumber == 12 ? currentPersianYear + 1 : currentPersianYear;
        var persianNextMonthStart = new PersianDateTime(nextYear, nextMonth, 1);

        var startDate = persianStartDate.ToDateTime();
        var nextMonthStart = persianNextMonthStart.ToDateTime();

        var endDate = nextMonthStart.AddSeconds(-1);

        return (startDate, endDate);
    }
}


