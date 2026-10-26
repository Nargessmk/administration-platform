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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestBed.Command;

public sealed class UpdateReservationRequestBedCommandHandler(
	IUnitOfWork unitOfWork,
	EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestBed> repository,
	EntityFrameworkCore.Repository.Interfaces.IRepository<ReservationRequestBedView> ReservationRequestBedViewRepository,
	IMapper mapper,
	IPublishEndpoint publishEndpoint,
	IElasticClient elasticClient,
	IDistributedCache distributedCache,
	IHttpContextAccessor httpContextAccessor,
	IAsyncDocumentSession dbSession)
	: BaseHandler<Models.ReservationRequestBed, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
		distributedCache, httpContextAccessor, dbSession),
		IRequestHandler<UpdateReservationRequestBedCommand, ResultDTO<ReservationRequestBedViewDTO>>
{
	public async Task<ResultDTO<ReservationRequestBedViewDTO>> Handle(UpdateReservationRequestBedCommand request, CancellationToken cancellationToken)
	{
		var existingQuery = _repository.SingleResultQuery().AndFilter(item => item.Id == request.ReservationRequestBed.Id);
		var existing = await _repository.SingleOrDefaultAsync(existingQuery, cancellationToken);
		
		var existingquery = ReservationRequestBedViewRepository.SingleResultQuery()
			.AndFilter(x => x.Id == existing.Id);
		var existingEntityView = await ReservationRequestBedViewRepository.SingleOrDefaultAsync(existingquery, cancellationToken);
		
		if (existing == null)
		{
			return ResultDTO<ReservationRequestBedViewDTO>.Failure(
				"رزرو تخت پیدا نشد",
				[]
			);
		}

		if (!existing.AllowUpdate)
		{
			return ResultDTO<ReservationRequestBedViewDTO>.Failure(
				"رزرو تخت قابل بروزرسانی نمی باشد",
				_mapper.Map<ReservationRequestBedViewDTO>(existingEntityView)
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

		existing.ReservationRequestHeaderId = request.ReservationRequestBed.ReservationRequestHeaderId;
		existing.BedId = request.ReservationRequestBed.BedId;
		existing.PersonnelId = request.ReservationRequestBed.PersonnelId;
		existing.ReservationDate = _mapper.Map<DateTime?>(request.ReservationRequestBed.ReservationDate);
		existing.CancellationStatus = request.ReservationRequestBed.CancellationStatus;
		existing.AdmissionStatus = request.ReservationRequestBed.AdmissionStatus;
		existing.RoomDeliveryStatus = request.ReservationRequestBed.RoomDeliveryStatus;
		existing.DeliveryDate = _mapper.Map<DateTime?>(request.ReservationRequestBed.DeliveryDate);
		existing.DeliveryTime = request.ReservationRequestBed.DeliveryTime;
		existing.DocumentReceipt = request.ReservationRequestBed.DocumentReceipt;

		var updated = _repository.Update(existing);
		await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
		
		var query = ReservationRequestBedViewRepository.SingleResultQuery()
			.AndFilter(x => x.Id == existing.Id);
		var entityView = await ReservationRequestBedViewRepository.SingleOrDefaultAsync(query, cancellationToken);

		if (entityView == null)
		{
			return ResultDTO<ReservationRequestBedViewDTO>.Failure(
				"رزرو تخت یافت نشد",
				[]
			);
		}

		return ResultDTO<ReservationRequestBedViewDTO>.Success(
			_mapper.Map<ReservationRequestBedViewDTO>(entityView),
			"رزرو تخت با موفقیت بروزرسانی شد"
		);
	}
}




