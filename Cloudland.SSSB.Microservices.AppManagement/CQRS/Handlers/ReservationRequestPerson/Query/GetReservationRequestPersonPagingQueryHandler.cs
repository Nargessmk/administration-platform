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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestPerson.Query;

public sealed class GetReservationRequestPersonPagingQueryHandler(
	IUnitOfWork unitOfWork,
	EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestPerson> repository,
	IMapper mapper,
	IPublishEndpoint publishEndpoint,
	IElasticClient elasticClient,
	IDistributedCache distributedCache,
	IHttpContextAccessor httpContextAccessor,
	IAsyncDocumentSession dbSession)
	: BaseHandler<Models.ReservationRequestPerson, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
		distributedCache, httpContextAccessor, dbSession),
		IRequestHandler<GetReservationRequestPersonPagingQuery, ResultDTO<PaginatedResult<ReservationRequestPersonDTO>>>
{
	public async Task<ResultDTO<PaginatedResult<ReservationRequestPersonDTO>>> Handle(GetReservationRequestPersonPagingQuery request, CancellationToken cancellationToken)
	{
		try
		{
			var query = _repository
				.MultipleResultQuery()
				.AndFilter(x => !x.Disabled && !x.hidden)
				.OrderBy(x => x.CreateAt);

			var entities = await _repository.SearchAsync(query, cancellationToken);
			var mappedEntities = entities.Select(x => _mapper.Map<ReservationRequestPersonDTO>(x));

			var paginatedResult = new PaginatedResult<ReservationRequestPersonDTO>(request.PageNumber, request.PageSize);
			await paginatedResult.PaginateFromEnumerable(mappedEntities);

			return ResultDTO<PaginatedResult<ReservationRequestPersonDTO>>.Success(
				paginatedResult,
				"لیست پرسنل رزرو با موفقیت دریافت شد"
			);
		}
		catch (Exception ex)
		{
			return ResultDTO<PaginatedResult<ReservationRequestPersonDTO>>.Failure(
				$"خطا در دریافت لیست پرسنل رزرو: {ex.Message}",
				[]
			);
		}
	}
}




