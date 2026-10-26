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

public sealed class GetReservationRequestLetterViewAllQueryHandler(
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
		IRequestHandler<GetReservationRequestLetterViewAllQuery, ResultDTO<List<ReservationRequestLetterViewDTO>>>
{
	public async Task<ResultDTO<List<ReservationRequestLetterViewDTO>>> Handle(
		GetReservationRequestLetterViewAllQuery request,
		CancellationToken cancellationToken)
	{
		try
		{
			var query = LetterViewRepository.MultipleResultQuery();
			var entities = await LetterViewRepository.SearchAsync(query, cancellationToken);

			var dtos = _mapper.Map<List<ReservationRequestLetterViewDTO>>(entities);

			return ResultDTO<List<ReservationRequestLetterViewDTO>>.Success(
				dtos,
				"لیست کامل نامه‌ها با موفقیت دریافت شد"
			);
		}
		catch (Exception ex)
		{
			return ResultDTO<List<ReservationRequestLetterViewDTO>>.Failure(
				$"خطا در دریافت لیست نامه‌ها: {ex.Message}",
				new List<ReservationRequestLetterViewDTO>()
			);
		}
	}
}



