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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestPerson.View;

public sealed class GetReservationRequestPersonViewAllPagingQueryHandler(
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
		IRequestHandler<GetReservationRequestPersonViewAllPagingQuery, ResultDTO<PaginatedResult<ReservationRequestPersonViewDTO>>>
{
	public async Task<ResultDTO<PaginatedResult<ReservationRequestPersonViewDTO>>> Handle(
		GetReservationRequestPersonViewAllPagingQuery request,
		CancellationToken cancellationToken)
	{
		try
		{
			var query = ReservationRequestPersonViewRepository.MultipleResultQuery();
			var entities = await ReservationRequestPersonViewRepository.SearchAsync(query, cancellationToken);
			var mappedEntities = entities.Select(x => _mapper.Map<ReservationRequestPersonViewDTO>(x));

			var paginatedResult = new PaginatedResult<ReservationRequestPersonViewDTO>(request.PageNumber, request.PageSize);
			await paginatedResult.PaginateFromEnumerable(mappedEntities);

			return ResultDTO<PaginatedResult<ReservationRequestPersonViewDTO>>.Success(
				paginatedResult,
				"لیست پرسنل رزرو با موفقیت دریافت شد"
			);
		}
		catch (Exception ex)
		{
			return ResultDTO<PaginatedResult<ReservationRequestPersonViewDTO>>.Failure(
				$"خطا در دریافت لیست پرسنل رزرو: {ex.Message}",
				[]
			);
		}
	}
}



