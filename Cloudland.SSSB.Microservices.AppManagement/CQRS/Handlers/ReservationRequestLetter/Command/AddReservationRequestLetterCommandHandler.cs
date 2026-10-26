using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestLetter.Command;

public sealed class AddReservationRequestLetterCommandHandler(
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
		IRequestHandler<AddReservationRequestLetterCommand, ResultDTO<ReservationRequestLetterDTO>>
{
public async Task<ResultDTO<ReservationRequestLetterDTO>> Handle(AddReservationRequestLetterCommand request, CancellationToken cancellationToken)
{
    if (_httpContextAccessor.HttpContext == null)
    {
        throw new Exception("HTTP context is not available");
    }
    
    var query = _repository.SingleResultQuery().AndFilter(item => item.ReservationRequestHeaderId == request.ReservationRequestLetter.ReservationRequestHeaderId);
    var duplicateEntity = await _repository.SingleOrDefaultAsync(query, cancellationToken);
    if (duplicateEntity != null)
    {
        return ResultDTO<ReservationRequestLetterDTO>.Failure(
            "برای این درخواست از قبل نامه ثبت شده است",
            request.ReservationRequestLetter
        ); 
    }

    var newEntity = _mapper.Map<Models.ReservationRequestLetter>(request.ReservationRequestLetter);
    newEntity.CreateAt = DateTime.Now;
    newEntity.LastModifiedAt = DateTime.Now;

    if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
    {
        var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
        if (Guid.TryParse(userClaim, out Guid userId))
        {
            newEntity.UserCreated = userId;
            newEntity.LastModifiedUser = userId;
        }
        else
        {
            throw new Exception("Invalid user ID in claims");
        }
    }
    else
    {
        throw new Exception("User ID claim not found");
    }

    newEntity.CustomerId = Guid.NewGuid();
    newEntity.AllowDelete = true;
    newEntity.AllowUpdate = true;
    newEntity.Disabled = false;
    newEntity.hidden = false;
    
    var entity = await _repository.AddAsync(newEntity, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken:cancellationToken);

    var existingHeaderQuery = headerRepository.SingleResultQuery().AndFilter(item => item.Id == request.ReservationRequestLetter.ReservationRequestHeaderId);
    var existing = await headerRepository.SingleOrDefaultAsync(existingHeaderQuery, cancellationToken);
    
    if (existing == null)
    {
        return ResultDTO<ReservationRequestLetterDTO>.Failure(
            "درخواست رزرو پیدا نشد",
            request.ReservationRequestLetter
        ); 
    }

    if (!existing.AllowUpdate)
    {
        return ResultDTO<ReservationRequestLetterDTO>.Failure(
            "درخواست رزرو قابل بروزرسانی نمی باشد",
            request.ReservationRequestLetter
        ); 
    }

    existing.RequestTypeId = new Guid("a3a5c680-17a0-4f4e-baf7-9516ef36f128");
    headerRepository.Update(existing);
    await _unitOfWork.SaveChangesAsync(cancellationToken:cancellationToken);
    
    return ResultDTO<ReservationRequestLetterDTO>.Success(
        _mapper.Map<ReservationRequestLetterDTO>(entity),
        "نامه با موفقیت ایجاد شد"
    );
}
}




