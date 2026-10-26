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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestPerson.View;

public sealed class GetReservationRequestPersonViewAllQueryHandler(
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
		IRequestHandler<GetReservationRequestPersonViewAllQuery, ResultDTO<List<ReservationRequestPersonViewDTO>>>
{
	public async Task<ResultDTO<List<ReservationRequestPersonViewDTO>>> Handle(
		GetReservationRequestPersonViewAllQuery request,
		CancellationToken cancellationToken)
	{
		try
		{
			var query = ReservationRequestPersonViewRepository.MultipleResultQuery();
			var entities = await ReservationRequestPersonViewRepository.SearchAsync(query, cancellationToken);

			var dtos = _mapper.Map<List<ReservationRequestPersonViewDTO>>(entities);

			return ResultDTO<List<ReservationRequestPersonViewDTO>>.Success(
				dtos,
				"لیست کامل پرسنل رزرو با موفقیت دریافت شد"
			);
		}
		catch (Exception ex)
		{
			return ResultDTO<List<ReservationRequestPersonViewDTO>>.Failure(
				$"خطا در دریافت لیست پرسنل رزرو: {ex.Message}",
				new List<ReservationRequestPersonViewDTO>()
			);
		}
	}
}



