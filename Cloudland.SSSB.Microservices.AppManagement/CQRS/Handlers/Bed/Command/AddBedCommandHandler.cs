using AutoMapper;
using Cloudland.SSSB.DataBaseContext;
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

public sealed class AddBedCommandHandler(
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
        IRequestHandler<AddBedCommand, ResultDTO<BedDTO>?>
{
    public async Task<ResultDTO<BedDTO>?> Handle(AddBedCommand request, CancellationToken cancellationToken)
    {

            if (_httpContextAccessor.HttpContext == null)
            {
                throw new Exception("HTTP context is not available");
            }
            
            // Check for duplicate title
            var queryDuplicate = _repository
                .SingleResultQuery()
                .AndFilter(item => item.BedNumber == request.Bed.BedNumber && item.RoomId == request.Bed.RoomId);
            var duplicate = await _repository.SingleOrDefaultAsync(queryDuplicate, cancellationToken);
            if (duplicate != null)
            {
                return ResultDTO<BedDTO>.Failure(
                    "تختی با این شماره برای این اتاق از قبل موجود می باشد",
                    _mapper.Map<BedDTO>(duplicate)
                );
            }

            // Map DTO to entity
            var newBed = _mapper.Map<Models.Bed>(request.Bed);

            newBed.CreateAt = DateTime.Now;
            newBed.LastModifiedAt = DateTime.Now;
        

            if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
            {
                var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
                if (Guid.TryParse(userClaim, out Guid userId))
                {
                    newBed.UserCreated = userId;
                    newBed.LastModifiedUser = userId;
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

            newBed.CustomerId = Guid.NewGuid();
            // Set default values
            newBed.AllowDelete = true;
            newBed.AllowUpdate = true;
            newBed.Disabled = false;
            newBed.hidden = false;

            // Add organizational chart to repository
            var bed = await _repository.AddAsync(newBed, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

            return ResultDTO<BedDTO>.Success(
                _mapper.Map<BedDTO>(bed),
                "تخت با موفقیت ایجاد شد"
            );
    }
}
