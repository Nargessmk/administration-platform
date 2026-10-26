using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestLetter.View;

public sealed class GetReservationRequestLetterViewByIdQueryHandler(
	IUnitOfWork unitOfWork,
	EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestLetter> repository,
	EntityFrameworkCore.Repository.Interfaces.IRepository<ReservationRequestLetterView> LetterViewRepository,
	IMapper mapper,
	IPublishEndpoint publishEndpoint,
	IElasticClient elasticClient,
	IDistributedCache distributedCache,
	IHttpContextAccessor httpContextAccessor,
	IAsyncDocumentSession dbSession)
	: BaseHandler<Models.ReservationRequestLetter, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
		distributedCache, httpContextAccessor, dbSession),
		IRequestHandler<GetReservationRequestLetterViewByIdQuery, ResultDTO<ReservationRequestLetterViewDTO>>
{
	public async Task<ResultDTO<ReservationRequestLetterViewDTO>> Handle(
		GetReservationRequestLetterViewByIdQuery request,
		CancellationToken cancellationToken)
	{
		try
		{
			var query = LetterViewRepository.SingleResultQuery()
				.AndFilter(x => x.Id == request.Id);
			var entity = await LetterViewRepository.SingleOrDefaultAsync(query, cancellationToken);

			if (entity == null)
			{
				return ResultDTO<ReservationRequestLetterViewDTO>.Failure(
					"نامه یافت نشد",
					[]
				);
			}

			var dto = _mapper.Map<ReservationRequestLetterViewDTO>(entity);

			return ResultDTO<ReservationRequestLetterViewDTO>.Success(
				dto,
				"نامه با موفقیت دریافت شد"
			);
		}
		catch (Exception ex)
		{
			return ResultDTO<ReservationRequestLetterViewDTO>.Failure(
				$"خطا در دریافت نامه: {ex.Message}",
				[]
			);
		}
	}
}



