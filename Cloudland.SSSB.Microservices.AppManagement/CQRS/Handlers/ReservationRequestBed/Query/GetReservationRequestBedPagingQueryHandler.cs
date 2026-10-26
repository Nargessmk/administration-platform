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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestBed.Query;

public sealed class GetReservationRequestBedPagingQueryHandler(
	IUnitOfWork unitOfWork,
	EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestBed> repository,
	IMapper mapper,
	IPublishEndpoint publishEndpoint,
	IElasticClient elasticClient,
	IDistributedCache distributedCache,
	IHttpContextAccessor httpContextAccessor,
	IAsyncDocumentSession dbSession)
	: BaseHandler<Models.ReservationRequestBed, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
		distributedCache, httpContextAccessor, dbSession),
		IRequestHandler<GetReservationRequestBedPagingQuery, ResultDTO<PaginatedResult<ReservationRequestBedDTO>>>
{
	public async Task<ResultDTO<PaginatedResult<ReservationRequestBedDTO>>> Handle(GetReservationRequestBedPagingQuery request, CancellationToken cancellationToken)
	{
		try
		{
			var query = _repository
				.MultipleResultQuery()
				.AndFilter(x => !x.Disabled && !x.hidden)
				.OrderBy(x => x.CreateAt);

			var entities = await _repository.SearchAsync(query, cancellationToken);
			var mappedEntities = entities.Select(x => _mapper.Map<ReservationRequestBedDTO>(x));

			var paginatedResult = new PaginatedResult<ReservationRequestBedDTO>(request.PageNumber, request.PageSize);
			await paginatedResult.PaginateFromEnumerable(mappedEntities);

			return ResultDTO<PaginatedResult<ReservationRequestBedDTO>>.Success(
				paginatedResult,
				"لیست رزرو تخت‌ها با موفقیت دریافت شد"
			);
		}
		catch (Exception ex)
		{
			return ResultDTO<PaginatedResult<ReservationRequestBedDTO>>.Failure(
				$"خطا در دریافت لیست رزرو تخت‌ها: {ex.Message}",
				[]
			);
		}
	}
}




