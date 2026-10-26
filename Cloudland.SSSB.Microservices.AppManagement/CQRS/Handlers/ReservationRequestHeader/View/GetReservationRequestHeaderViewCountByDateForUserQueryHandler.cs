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

public sealed class GetReservationRequestHeaderViewCountByDateForUserQueryHandler(
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
        IRequestHandler<GetReservationRequestHeaderViewCountByDateForUserQuery, ResultDTO<int>>
{
    public async Task<ResultDTO<int>> Handle(
        GetReservationRequestHeaderViewCountByDateForUserQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get user ID from claims
            if (_httpContextAccessor.HttpContext == null)
            {
                return ResultDTO<int>.Failure(
                    "HTTP context is not available",
                    0
                );
            }

            Guid userId;
            if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
            {
                var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
                if (!Guid.TryParse(userClaim, out userId))
                {
                    return ResultDTO<int>.Failure(
                        "شناسه کاربر نامعتبر است",
                        0
                    );
                }
            }
            else
            {
                return ResultDTO<int>.Failure(
                    "شناسه کاربر یافت نشد",
                    0
                );
            }

            // Convert PersianDate to DateTime using AutoMapper
            var targetDate = _mapper.Map<DateTime?>(request.PersianDate);
            
            if (targetDate == null)
            {
                return ResultDTO<int>.Failure(
                    "تاریخ شمسی نامعتبر است",
                    0
                );
            }

            // Get the start and end of the target date (to compare only the date part, not time)
            var startOfDay = targetDate.Value.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            var query = ReservationRequestHeaderViewRepository.MultipleResultQuery()
                .AndFilter(x => x.CreateAt >= startOfDay && x.CreateAt <= endOfDay)
                .AndFilter(x => x.UserCreated == userId);

            var entities = await ReservationRequestHeaderViewRepository.SearchAsync(query, cancellationToken);
            var count = entities.Count();

            return ResultDTO<int>.Success(
                count,
                $"تعداد درخواست‌های رزرو ایجاد شده توسط کاربر در تاریخ {request.PersianDate}: {count}"
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

