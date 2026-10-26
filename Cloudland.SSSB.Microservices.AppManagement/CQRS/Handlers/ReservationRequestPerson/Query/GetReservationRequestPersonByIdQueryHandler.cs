using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestPerson.Query;

public sealed class GetReservationRequestPersonByIdQueryHandler(
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
		IRequestHandler<GetReservationRequestPersonByIdQuery, ResultDTO<ReservationRequestPersonDTO>>
{
	public async Task<ResultDTO<ReservationRequestPersonDTO>> Handle(GetReservationRequestPersonByIdQuery request, CancellationToken cancellationToken)
	{
		var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
		var entity = await _repository.SingleOrDefaultAsync(query, cancellationToken);
		if (entity == null)
		{
			throw new InvalidOperationException("رکورد یافت نشد");
		}
		return ResultDTO<ReservationRequestPersonDTO>.Success(_mapper.Map<ReservationRequestPersonDTO>(entity));
	}
}




