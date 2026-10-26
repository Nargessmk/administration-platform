using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestLetter.Command;

public sealed class UpdateReservationRequestLetterCommandHandler(
	IUnitOfWork unitOfWork,
	EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestLetter> repository,
	IMapper mapper,
	IPublishEndpoint publishEndpoint,
	IElasticClient elasticClient,
	IDistributedCache distributedCache,
	IHttpContextAccessor httpContextAccessor,
	IAsyncDocumentSession dbSession)
	: BaseHandler<Models.ReservationRequestLetter, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
		distributedCache, httpContextAccessor, dbSession),
		IRequestHandler<UpdateReservationRequestLetterCommand, ResultDTO<ReservationRequestLetterDTO>>
{
	public async Task<ResultDTO<ReservationRequestLetterDTO>> Handle(UpdateReservationRequestLetterCommand request, CancellationToken cancellationToken)
	{
		var existingQuery = _repository.SingleResultQuery().AndFilter(item => item.Id == request.ReservationRequestLetter.Id);
		var existing = await _repository.SingleOrDefaultAsync(existingQuery, cancellationToken);
		if (existing == null)
		{
			return ResultDTO<ReservationRequestLetterDTO>.Failure(
				"نامه پیدا نشد",
				request.ReservationRequestLetter
			);
		}

		if (!existing.AllowUpdate)
		{
			return ResultDTO<ReservationRequestLetterDTO>.Failure(
				"نامه قابل بروزرسانی نمی باشد",
				request.ReservationRequestLetter
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

		existing.ReservationRequestHeaderId = request.ReservationRequestLetter.ReservationRequestHeaderId;
		existing.LetterDate = _mapper.Map<DateTime?>(request.ReservationRequestLetter.LetterDate);
		existing.LetterNumber = request.ReservationRequestLetter.LetterNumber;
		existing.LetterSubject = request.ReservationRequestLetter.LetterSubject;
		existing.LetterFileUrl = request.ReservationRequestLetter.LetterFileUrl;

		var updated = _repository.Update(existing);
		await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

		return ResultDTO<ReservationRequestLetterDTO>.Success(
			_mapper.Map<ReservationRequestLetterDTO>(updated),
			"نامه با موفقیت بروزرسانی شد"
		);
	}
}




