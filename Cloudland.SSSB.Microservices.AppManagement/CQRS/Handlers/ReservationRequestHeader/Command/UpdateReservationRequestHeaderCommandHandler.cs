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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestHeader.Command;

public sealed class UpdateReservationRequestHeaderCommandHandler(
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
        IRequestHandler<UpdateReservationRequestHeaderCommand, ResultDTO<ReservationRequestHeaderViewDTO>>
{
    public async Task<ResultDTO<ReservationRequestHeaderViewDTO>> Handle(UpdateReservationRequestHeaderCommand request, CancellationToken cancellationToken)
    {
        var existingQuery = _repository.SingleResultQuery().AndFilter(item => item.Id == request.ReservationRequestHeader.Id);
        var existing = await _repository.SingleOrDefaultAsync(existingQuery, cancellationToken);
        
        var existingquery = ReservationRequestHeaderViewRepository.SingleResultQuery()
            .AndFilter(x => x.Id == existing.Id);
        var existingEntityView = await ReservationRequestHeaderViewRepository.SingleOrDefaultAsync(existingquery, cancellationToken);
        
        if (existing == null)
        {
            return ResultDTO<ReservationRequestHeaderViewDTO>.Failure(
                "درخواست رزرو پیدا نشد",
                []
            ); 
        }

        if (!existing.AllowUpdate)
        {
            return ResultDTO<ReservationRequestHeaderViewDTO>.Failure(
                "درخواست رزرو قابل بروزرسانی نمی باشد",
                _mapper.Map<ReservationRequestHeaderViewDTO>(existingEntityView)
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

        existing.FromDate = _mapper.Map<DateTime?>(request.ReservationRequestHeader.FromDate);
        existing.ToDate = _mapper.Map<DateTime?>(request.ReservationRequestHeader.ToDate);
        existing.RequestTypeId = request.ReservationRequestHeader.RequestTypeId;
        existing.AccountNumberId = request.ReservationRequestHeader.AccountNumberId;
        existing.FromDepartmentId = request.ReservationRequestHeader.FromDepartmentId;
        existing.ForDepartmentId = request.ReservationRequestHeader.ForDepartmentId;
        existing.RequestingOrganizationalUnitId = request.ReservationRequestHeader.RequestingOrganizationalUnitId;
        existing.VehicleId = request.ReservationRequestHeader.VehicleId;
        existing.DriverId = request.ReservationRequestHeader.DriverId;
        existing.ShiftChangeDayId = request.ReservationRequestHeader.ShiftChangeDayId;
        existing.LocationId = request.ReservationRequestHeader.LocationId;
        existing.desc = request.ReservationRequestHeader.desc;

        var updated = _repository.Update(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
        var query = ReservationRequestHeaderViewRepository.SingleResultQuery()
            .AndFilter(x => x.Id == existing.Id);
        var entityView = await ReservationRequestHeaderViewRepository.SingleOrDefaultAsync(query, cancellationToken);

        if (entityView == null)
        {
            return ResultDTO<ReservationRequestHeaderViewDTO>.Failure(
                "درخواست رزرو یافت نشد",
                []
            );
        }

        return ResultDTO<ReservationRequestHeaderViewDTO>.Success(
            _mapper.Map<ReservationRequestHeaderViewDTO>(entityView),
            "درخواست رزرو با موفقیت بروزرسانی شد"
        );
    }
}



