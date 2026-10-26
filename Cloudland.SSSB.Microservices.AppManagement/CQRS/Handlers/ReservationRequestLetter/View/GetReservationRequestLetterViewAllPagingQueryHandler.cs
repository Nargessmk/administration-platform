using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestLetter.View;

public sealed class GetReservationRequestLetterViewAllPagingQueryHandler(
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
		IRequestHandler<GetReservationRequestLetterViewAllPagingQuery, ResultDTO<PaginatedResult<ReservationRequestLetterViewDTO>>>
{
	public async Task<ResultDTO<PaginatedResult<ReservationRequestLetterViewDTO>>> Handle(
		GetReservationRequestLetterViewAllPagingQuery request,
		CancellationToken cancellationToken)
	{
		try
		{
			var query = LetterViewRepository.MultipleResultQuery();
			var entities = await LetterViewRepository.SearchAsync(query, cancellationToken);
			var mappedEntities = entities.Select(x => _mapper.Map<ReservationRequestLetterViewDTO>(x));

			var paginatedResult = new PaginatedResult<ReservationRequestLetterViewDTO>(request.PageNumber, request.PageSize);
			await paginatedResult.PaginateFromEnumerable(mappedEntities);

			return ResultDTO<PaginatedResult<ReservationRequestLetterViewDTO>>.Success(
				paginatedResult,
				"لیست نامه‌ها با موفقیت دریافت شد"
			);
		}
		catch (Exception ex)
		{
			return ResultDTO<PaginatedResult<ReservationRequestLetterViewDTO>>.Failure(
				$"خطا در دریافت لیست نامه‌ها: {ex.Message}",
				[]
			);
		}
	}
}



