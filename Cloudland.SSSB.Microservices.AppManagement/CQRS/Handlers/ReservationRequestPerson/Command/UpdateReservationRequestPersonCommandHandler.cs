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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestPerson.Command;

public sealed class UpdateReservationRequestPersonCommandHandler(
	IUnitOfWork unitOfWork,
	EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestPerson> repository,
	EntityFrameworkCore.Repository.Interfaces.IRepository<ReservationRequestPersonView> ReservationRequestPersonViewRepository,
	IMapper mapper,
	IPublishEndpoint publishEndpoint,
	IElasticClient elasticClient,
	IDistributedCache distributedCache,
	IHttpContextAccessor httpContextAccessor,
	IAsyncDocumentSession dbSession)
	: BaseHandler<Models.ReservationRequestPerson, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
		distributedCache, httpContextAccessor, dbSession),
		IRequestHandler<UpdateReservationRequestPersonCommand, ResultDTO<ReservationRequestPersonViewDTO>>
{
	public async Task<ResultDTO<ReservationRequestPersonViewDTO>> Handle(UpdateReservationRequestPersonCommand request, CancellationToken cancellationToken)
	{
		var existingQuery = _repository.SingleResultQuery().AndFilter(item => item.Id == request.ReservationRequestPerson.Id);
		var existing = await _repository.SingleOrDefaultAsync(existingQuery, cancellationToken);
		
		var existingquery = ReservationRequestPersonViewRepository.SingleResultQuery()
			.AndFilter(x => x.Id == existing.Id);
		var existingEntityView = await ReservationRequestPersonViewRepository.SingleOrDefaultAsync(existingquery, cancellationToken);
		
		if (existing == null)
		{
			return ResultDTO<ReservationRequestPersonViewDTO>.Failure(
				"پرسنل رزرو پیدا نشد",
				[]
			);
		}

		if (!existing.AllowUpdate)
		{
			return ResultDTO<ReservationRequestPersonViewDTO>.Failure(
				"پرسنل رزرو قابل بروزرسانی نمی باشد",
				_mapper.Map<ReservationRequestPersonViewDTO>(existingEntityView)
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

		existing.PersonnelId = request.ReservationRequestPerson.PersonnelId;
		existing.ReservationRequestHeaderId = request.ReservationRequestPerson.ReservationRequestHeaderId;

		var updated = _repository.Update(existing);
		await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
		
		var query = ReservationRequestPersonViewRepository.SingleResultQuery()
			.AndFilter(x => x.Id == existing.Id);
		var entityView = await ReservationRequestPersonViewRepository.SingleOrDefaultAsync(query, cancellationToken);

		if (entityView == null)
		{
			return ResultDTO<ReservationRequestPersonViewDTO>.Failure(
				"پرسنل رزرو یافت نشد",
				[]
			);
		}

		return ResultDTO<ReservationRequestPersonViewDTO>.Success(
			_mapper.Map<ReservationRequestPersonViewDTO>(entityView),
			"پرسنل رزرو با موفقیت بروزرسانی شد"
		);
	}
}




