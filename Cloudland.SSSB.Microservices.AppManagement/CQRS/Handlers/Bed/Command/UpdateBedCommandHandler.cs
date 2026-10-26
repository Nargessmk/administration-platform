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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Bed.Command;

public sealed class UpdateBedCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Bed> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Bed, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<UpdateBedCommand, ResultDTO<BedDTO>?>
{


    public async Task<ResultDTO<BedDTO>?> Handle(UpdateBedCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Bed.Id);
        var existingBed = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        
        if (existingBed == null)
        {
            return ResultDTO<BedDTO>.Failure(
                "تخت پیدا نشد",
                []
            ); 
        }
        
        if (!existingBed.AllowUpdate)
        {
            return ResultDTO<BedDTO>.Failure(
                "تخت قابل بروزرسانی نمی باشد",
                request.Bed
            ); 
        }

        // Check for duplicate Bed number 
        if (!string.IsNullOrEmpty(request.Bed.BedNumber) && request.Bed.BedNumber != existingBed.BedNumber)
        {
            var duplicateQuery = _repository
                .SingleResultQuery()
                .AndFilter(item => item.BedNumber == request.Bed.BedNumber 
                                   && item.RoomId == request.Bed.RoomId
                                   && item.Id != existingBed.Id);
            var duplicate = await _repository.SingleOrDefaultAsync(duplicateQuery, cancellationToken);
            if (duplicate != null) 
            {
                return ResultDTO<BedDTO>.Failure(
                    "تخت با این شماره وجود دارد", 
                    _mapper.Map<BedDTO>(duplicate) // Optionally return the duplicate
                );
            }
        }

        existingBed.LastModifiedAt = DateTime.Now;
        
        // Get user ID from claims
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                existingBed.LastModifiedUser = userId;
            }
        }

        existingBed.title = request.Bed.title;
        existingBed.desc = request.Bed.desc;
        existingBed.BedStatusId = request.Bed.BedStatusId;
        existingBed.BedNumber = request.Bed.BedNumber;
        existingBed.RoomId = request.Bed.RoomId;
        existingBed.Capacity = request.Bed.Capacity;
        existingBed.BedReservationStatusId = request.Bed.BedReservationStatusId;
        existingBed.ReservationId = request.Bed.ReservationId;

        
        
        var updatedBed = _repository.Update(existingBed);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        return ResultDTO<BedDTO>.Success(
            _mapper.Map<BedDTO>(updatedBed),
            $"تخت {updatedBed.title} با موفقیت بروزرسانی شد"
        );

    }
}
