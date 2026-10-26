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

public sealed class GetReservationRequestPersonViewByIdQueryHandler(
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
		IRequestHandler<GetReservationRequestPersonViewByIdQuery, ResultDTO<ReservationRequestPersonViewDTO>>
{
	public async Task<ResultDTO<ReservationRequestPersonViewDTO>> Handle(
		GetReservationRequestPersonViewByIdQuery request,
		CancellationToken cancellationToken)
	{
		try
		{
			var query = ReservationRequestPersonViewRepository.SingleResultQuery()
				.AndFilter(x => x.Id == request.Id);
			var entity = await ReservationRequestPersonViewRepository.SingleOrDefaultAsync(query, cancellationToken);

			if (entity == null)
			{
				return ResultDTO<ReservationRequestPersonViewDTO>.Failure(
					"پرسنل رزرو یافت نشد",
					[]
				);
			}

			var dto = _mapper.Map<ReservationRequestPersonViewDTO>(entity);

			return ResultDTO<ReservationRequestPersonViewDTO>.Success(
				dto,
				"پرسنل رزرو با موفقیت دریافت شد"
			);
		}
		catch (Exception ex)
		{
			return ResultDTO<ReservationRequestPersonViewDTO>.Failure(
				$"خطا در دریافت پرسنل رزرو: {ex.Message}",
				[]
			);
		}
	}
}



