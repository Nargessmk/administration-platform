using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestLetter.Query;

public sealed class GetReservationRequestLetterPagingQueryHandler(
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
		IRequestHandler<GetReservationRequestLetterPagingQuery, ResultDTO<PaginatedResult<ReservationRequestLetterDTO>>>
{
	public async Task<ResultDTO<PaginatedResult<ReservationRequestLetterDTO>>> Handle(GetReservationRequestLetterPagingQuery request, CancellationToken cancellationToken)
	{
		try
		{
			var query = _repository
				.MultipleResultQuery()
				.AndFilter(x => !x.Disabled && !x.hidden)
				.OrderBy(x => x.CreateAt);

			var entities = await _repository.SearchAsync(query, cancellationToken);
			var mappedEntities = entities.Select(x => _mapper.Map<ReservationRequestLetterDTO>(x));

			var paginatedResult = new PaginatedResult<ReservationRequestLetterDTO>(request.PageNumber, request.PageSize);
			await paginatedResult.PaginateFromEnumerable(mappedEntities);

			return ResultDTO<PaginatedResult<ReservationRequestLetterDTO>>.Success(
				paginatedResult,
				"لیست نامه‌ها با موفقیت دریافت شد"
			);
		}
		catch (Exception ex)
		{
			return ResultDTO<PaginatedResult<ReservationRequestLetterDTO>>.Failure(
				$"خطا در دریافت لیست نامه‌ها: {ex.Message}",
				[]
			);
		}
	}
}




