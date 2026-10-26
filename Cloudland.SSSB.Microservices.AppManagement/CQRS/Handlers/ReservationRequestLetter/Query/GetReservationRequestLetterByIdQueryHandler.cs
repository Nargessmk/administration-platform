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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestLetter.Query;

public sealed class GetReservationRequestLetterByIdQueryHandler(
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
		IRequestHandler<GetReservationRequestLetterByIdQuery, ResultDTO<ReservationRequestLetterDTO>>
{
	public async Task<ResultDTO<ReservationRequestLetterDTO>> Handle(GetReservationRequestLetterByIdQuery request, CancellationToken cancellationToken)
	{
		var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
		var entity = await _repository.SingleOrDefaultAsync(query, cancellationToken);
		if (entity == null)
		{
			throw new InvalidOperationException("رکورد یافت نشد");
		}
		return ResultDTO<ReservationRequestLetterDTO>.Success(_mapper.Map<ReservationRequestLetterDTO>(entity));
	}
}




