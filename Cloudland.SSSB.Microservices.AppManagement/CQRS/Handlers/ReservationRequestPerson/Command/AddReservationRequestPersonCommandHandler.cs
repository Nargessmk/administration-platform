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

public sealed class AddReservationRequestPersonCommandHandler(
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
		IRequestHandler<AddReservationRequestPersonCommand, ResultDTO<ReservationRequestPersonViewDTO>>
{
	public async Task<ResultDTO<ReservationRequestPersonViewDTO>> Handle(AddReservationRequestPersonCommand request, CancellationToken cancellationToken)
	{
		if (_httpContextAccessor.HttpContext == null)
		{
			throw new Exception("HTTP context is not available");
		}

		var newEntity = _mapper.Map<Models.ReservationRequestPerson>(request.ReservationRequestPerson);
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

		var query = ReservationRequestPersonViewRepository.SingleResultQuery()
			.AndFilter(x => x.Id == entity.Id);
		var entityView = await ReservationRequestPersonViewRepository.SingleOrDefaultAsync(query, cancellationToken);

		if (entityView == null)
		{
			return ResultDTO<ReservationRequestPersonViewDTO>.Failure(
				"پرسنل رزرو یافت نشد",
				[]
			);
		}

		var dto = _mapper.Map<ReservationRequestPersonViewDTO>(entityView);

		return ResultDTO<ReservationRequestPersonViewDTO>.Success(
			dto,
			"پرسنل رزرو با موفقیت ایجاد شد"
		);
	}
}




