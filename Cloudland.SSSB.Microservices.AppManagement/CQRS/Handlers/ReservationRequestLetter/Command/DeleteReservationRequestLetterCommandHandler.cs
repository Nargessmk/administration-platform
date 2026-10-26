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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestLetter.Command;

public sealed class DeleteReservationRequestLetterCommandHandler(
	IUnitOfWork unitOfWork,
	EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestLetter> repository,
	EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestHeader> headerRepository,
	IMapper mapper,
	IPublishEndpoint publishEndpoint,
	IElasticClient elasticClient,
	IDistributedCache distributedCache,
	IHttpContextAccessor httpContextAccessor,
	IAsyncDocumentSession dbSession)
	: BaseHandler<Models.ReservationRequestLetter, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
		distributedCache, httpContextAccessor, dbSession),
		IRequestHandler<DeleteReservationRequestLetterCommand, ResultDTO<int>>
{
	public async Task<ResultDTO<int>> Handle(DeleteReservationRequestLetterCommand request, CancellationToken cancellationToken)
	{
		var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
		var entity = await _repository.SingleOrDefaultAsync(query, cancellationToken);
		if (entity == null)
		{
			return ResultDTO<int>.Failure(
				$"نامه با این شناسه برای حذف پیدا نشد",
				0
			);
		}
		
		await _repository.RemoveAsync(x => x.Id == request.Id, cancellationToken);
		var rows = await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
		
		var existingHeaderQuery = headerRepository.SingleResultQuery().AndFilter(item => item.Id == entity.ReservationRequestHeaderId);
		var existing = await headerRepository.SingleOrDefaultAsync(existingHeaderQuery, cancellationToken);
    
		if (existing == null)
		{
			return ResultDTO<int>.Failure(
				"درخواست رزرو پیدا نشد",0); 
		}

		if (!existing.AllowUpdate)
		{
			return ResultDTO<int>.Failure(
				"درخواست رزرو قابل بروزرسانی نمی باشد",
				0
			); 
		}

		existing.RequestTypeId = new Guid("5033114e-86f5-42b9-bc17-e09a92dcbfaa");
		headerRepository.Update(existing);
		await _unitOfWork.SaveChangesAsync(cancellationToken:cancellationToken);
		return ResultDTO<int>.Success(rows, "نامه با موفقیت حذف شد");
	}
}




