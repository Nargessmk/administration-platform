using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Models.View;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationFoodCalendar.Command;

public sealed class UpdateReservationFoodCalendarCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationFoodCalendar> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ReservationFoodCalendarView> ReservationFoodCalendarViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ReservationFoodCalendar, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
        distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<UpdateReservationFoodCalendarCommand, ResultDTO<ReservationFoodCalendarViewDTO>>
{
    public async Task<ResultDTO<ReservationFoodCalendarViewDTO>> Handle(UpdateReservationFoodCalendarCommand request, CancellationToken cancellationToken)
    {
        var existingQuery = _repository.SingleResultQuery().AndFilter(item => item.Id == request.ReservationFoodCalendar.Id);
        var existing = await _repository.SingleOrDefaultAsync(existingQuery, cancellationToken);
        
        var existingquery = ReservationFoodCalendarViewRepository.SingleResultQuery()
            .AndFilter(x => x.Id == existing.Id);
        var existingEntityView = await ReservationFoodCalendarViewRepository.SingleOrDefaultAsync(existingquery, cancellationToken);
        
        if (existing == null)
        {
            return ResultDTO<ReservationFoodCalendarViewDTO>.Failure(
                "تقویم غذای رزرو پیدا نشد",
                []
            );
        }

        if (!existing.AllowUpdate)
        {
            return ResultDTO<ReservationFoodCalendarViewDTO>.Failure(
                "تقویم غذای رزرو قابل بروزرسانی نمی باشد",
                _mapper.Map<ReservationFoodCalendarViewDTO>(existingEntityView)
            );
        }

        existing.LastModifiedAt = DateTime.Now;

        if (_httpContextAccessor.HttpContext?.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                existing.LastModifiedUser = userId;
            }
        }

        existing.ReservationRequestHeaderId = request.ReservationFoodCalendar.ReservationRequestHeaderId;
        existing.PersonnelId = request.ReservationFoodCalendar.PersonnelId;
        existing.ContractFoodCalendarId = request.ReservationFoodCalendar.ContractFoodCalendarId;
        existing.ReservationDate = _mapper.Map<DateTime?>(request.ReservationFoodCalendar.ReservationDate);
        existing.Quantity = request.ReservationFoodCalendar.Quantity;
        existing.CancelationStatus = request.ReservationFoodCalendar.CancelationStatus;
        
        var updated = _repository.Update(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
        var query = ReservationFoodCalendarViewRepository.SingleResultQuery()
            .AndFilter(x => x.Id == existing.Id);
        var entityView = await ReservationFoodCalendarViewRepository.SingleOrDefaultAsync(query, cancellationToken);

        if (entityView == null)
        {
            return ResultDTO<ReservationFoodCalendarViewDTO>.Failure(
                "تقویم غذای رزرو یافت نشد",
                []
            );
        }

        return ResultDTO<ReservationFoodCalendarViewDTO>.Success(
            _mapper.Map<ReservationFoodCalendarViewDTO>(entityView),
            "تقویم غذای رزرو با موفقیت بروزرسانی شد"
        );
    }
}
