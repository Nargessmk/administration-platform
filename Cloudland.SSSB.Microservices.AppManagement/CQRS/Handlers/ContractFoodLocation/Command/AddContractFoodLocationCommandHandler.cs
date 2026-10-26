using AutoMapper;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFoodLocation.Command;

public sealed class AddContractFoodLocationCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFoodLocation> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractFoodLocation, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddContractFoodLocationCommand, ResultDTO<ContractFoodLocationDTO>?>
{
    public async Task<ResultDTO<ContractFoodLocationDTO>?> Handle(AddContractFoodLocationCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }

        var dbContext = _unitOfWork.DbContext as SSSBDBContext;
        if (dbContext == null)
        {
            throw new Exception("SSSB database context is not available");
        }

        // Avoid duplicate ContractFood-ContractLocation pair
        if (request.ContractFoodLocation.ContractFoodId.HasValue && request.ContractFoodLocation.ContractLocationId.HasValue)
        {
            var duplicate = await dbContext.ContractFoodLocations
                .Where(x => !x.Disabled && !x.hidden)
                .Where(x => x.ContractFoodId == request.ContractFoodLocation.ContractFoodId && x.ContractLocationId == request.ContractFoodLocation.ContractLocationId)
                .FirstOrDefaultAsync(cancellationToken);
            if (duplicate != null)
            {
                return ResultDTO<ContractFoodLocationDTO>.Failure(
                    "این رابطه غذای قرارداد-مکان قبلاً ایجاد شده است",
                    _mapper.Map<ContractFoodLocationDTO>(duplicate)
                );
            }
        }

        var newEntity = _mapper.Map<Models.ContractFoodLocation>(request.ContractFoodLocation);

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

        var added = await _repository.AddAsync(newEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        return ResultDTO<ContractFoodLocationDTO>.Success(
            _mapper.Map<ContractFoodLocationDTO>(added),
            "رابطه غذای قرارداد-مکان با موفقیت ایجاد شد"
        );
    }
}
