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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractLocation.Command;

public sealed class AddContractLocationCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractLocation> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFood> contractFoodRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFoodLocation> contractFoodLocationRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractLocation, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddContractLocationCommand, ResultDTO<ContractLocationDTO>?>
{
    public async Task<ResultDTO<ContractLocationDTO>?> Handle(AddContractLocationCommand request, CancellationToken cancellationToken)
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

        // Avoid duplicate Contract-Location pair
        if (request.ContractLocation.ContractId.HasValue && request.ContractLocation.LocationId.HasValue)
        {
            var duplicate = await dbContext.ContractLocations
                .Where(x => !x.Disabled && !x.hidden)
                .Where(x => x.ContractId == request.ContractLocation.ContractId && x.LocationId == request.ContractLocation.LocationId)
                .FirstOrDefaultAsync(cancellationToken);
            if (duplicate != null)
            {
                return ResultDTO<ContractLocationDTO>.Failure(
                    "این مکان قبلاً به این قرارداد اختصاص یافته است",
                    _mapper.Map<ContractLocationDTO>(duplicate)
                );
            }
        }

        var newEntity = _mapper.Map<Models.ContractLocation>(request.ContractLocation);

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

        // Create ContractFoodLocation entries for all existing ContractFoods of the same contract
        if (added.ContractId.HasValue)
        {
            var foodsQuery = contractFoodRepository
                .MultipleResultQuery()
                .AndFilter(x => !x.Disabled && !x.hidden && x.ContractId == added.ContractId);
            var contractFoods = await contractFoodRepository.SearchAsync(foodsQuery, cancellationToken);

            Guid? currentUserId = null;
            if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
            {
                var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
                if (Guid.TryParse(userClaim, out Guid parsedUserId))
                {
                    currentUserId = parsedUserId;
                }
            }

            foreach (var contractFood in contractFoods)
            {
                var existingLinkQuery = contractFoodLocationRepository
                    .MultipleResultQuery()
                    .AndFilter(x => !x.Disabled && !x.hidden && x.ContractFoodId == contractFood.Id && x.ContractLocationId == added.Id);
                var existingLinks = await contractFoodLocationRepository.SearchAsync(existingLinkQuery, cancellationToken);
                if (existingLinks.Any())
                {
                    continue;
                }

                var link = new Models.ContractFoodLocation
                {
                    Id = Guid.NewGuid(),
                    ContractFoodId = contractFood.Id,
                    ContractLocationId = added.Id,
                    CreateAt = DateTime.Now,
                    LastModifiedAt = DateTime.Now,
                    UserCreated = currentUserId ?? Guid.Empty,
                    LastModifiedUser = currentUserId ?? Guid.Empty,
                    CustomerId = Guid.NewGuid(),
                    AllowDelete = true,
                    AllowUpdate = true,
                    Disabled = false,
                    hidden = false,
                    title = " "
                };

                await contractFoodLocationRepository.AddAsync(link, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        }

        return ResultDTO<ContractLocationDTO>.Success(
            _mapper.Map<ContractLocationDTO>(added),
            "رابطه مکان-قرارداد با موفقیت ایجاد شد"
        );
    }
}


