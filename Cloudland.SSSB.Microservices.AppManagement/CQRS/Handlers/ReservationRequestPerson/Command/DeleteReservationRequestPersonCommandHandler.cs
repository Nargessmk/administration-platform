using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestPerson.Command;

public sealed class DeleteReservationRequestPersonCommandHandler(
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
		IRequestHandler<DeleteReservationRequestPersonCommand, ResultDTO<int>>
{
	public async Task<ResultDTO<int>> Handle(DeleteReservationRequestPersonCommand request, CancellationToken cancellationToken)
	{
		var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
		var entity = await _repository.SingleOrDefaultAsync(query, cancellationToken);
		if (entity == null)
		{
			return ResultDTO<int>.Failure(
				$"پرسنل رزرو با این شناسه برای حذف پیدا نشد",
				0
			);
		}

		await _repository.RemoveAsync(x => x.Id == request.Id, cancellationToken);
		var rows = await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
		return ResultDTO<int>.Success(rows, "پرسنل رزرو با موفقیت حذف شد");
	}
}




