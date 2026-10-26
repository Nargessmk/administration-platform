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

public sealed class AddReservationRequestBedCommandHandler(
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
		IRequestHandler<AddReservationRequestBedCommand, ResultDTO<ReservationRequestBedViewDTO>>
{
	public async Task<ResultDTO<ReservationRequestBedViewDTO>> Handle(AddReservationRequestBedCommand request, CancellationToken cancellationToken)
	{
		if (_httpContextAccessor.HttpContext == null)
		{
			throw new Exception("HTTP context is not available");
		}

		var newEntity = _mapper.Map<Models.ReservationRequestBed>(request.ReservationRequestBed);
		newEntity.CreateAt = DateTime.Now;
		newEntity.LastModifiedAt = DateTime.Now;

		if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
		{
			var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
			if (Guid.TryParse(userClaim, out Guid userId))
			{
				newEntity.UserCreated = userId;
				newEntity.LastModifiedUser = userId;
			}
			else
			{
				throw new Exception("Invalid user ID in claims");
			}
		}
		else
		{
			throw new Exception("User ID claim not found");
		}

		newEntity.CustomerId = Guid.NewGuid();
		newEntity.AllowDelete = true;
		newEntity.AllowUpdate = true;
		newEntity.Disabled = false;
		newEntity.hidden = false;

		var entity = await _repository.AddAsync(newEntity, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

		var query = ReservationRequestBedViewRepository.SingleResultQuery()
			.AndFilter(x => x.Id == entity.Id);
		var entityView = await ReservationRequestBedViewRepository.SingleOrDefaultAsync(query, cancellationToken);

		if (entityView == null)
		{
			return ResultDTO<ReservationRequestBedViewDTO>.Failure(
				"رزرو تخت یافت نشد",
				[]
			);
		}

		var dto = _mapper.Map<ReservationRequestBedViewDTO>(entityView);

		return ResultDTO<ReservationRequestBedViewDTO>.Success(
			dto,
			"رزرو تخت با موفقیت ایجاد شد"
		);
	}
}




