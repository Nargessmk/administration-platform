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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFood.Command;

public sealed class UpdateContractFoodCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFood> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractLocation> contractLocationRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFoodLocation> contractFoodLocationRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractFood, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<UpdateContractFoodCommand, ResultDTO<ContractFoodDTO>?>
{
    public async Task<ResultDTO<ContractFoodDTO>?> Handle(UpdateContractFoodCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }

        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.ContractFood.Id);
        var existing = await _repository.SingleOrDefaultAsync(query, cancellationToken);

        if (existing == null)
        {
            return ResultDTO<ContractFoodDTO>.Failure(
                "رابطه غذا-قرارداد پیدا نشد",
                []
            );
        }

        if (!existing.AllowUpdate)
        {
            return ResultDTO<ContractFoodDTO>.Failure(
                "این رکورد قابل بروزرسانی نمی باشد",
                request.ContractFood
            );
        }

        existing.LastModifiedAt = DateTime.Now;
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                existing.LastModifiedUser = userId;
            }
        }

        existing.ContractId = request.ContractFood.ContractId;
        existing.FoodId = request.ContractFood.FoodId;
        existing.Quantity = request.ContractFood.Quantity;
        existing.Price = request.ContractFood.Price;
        existing.FreePrice = request.ContractFood.FreePrice;



        var updated = _repository.Update(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Ensure links exist to all locations of the same contract
        if (updated.ContractId.HasValue)
        {
            var locationsQuery = contractLocationRepository
                .MultipleResultQuery()
                .AndFilter(x => !x.Disabled && !x.hidden && x.ContractId == updated.ContractId);
            var contractLocations = await contractLocationRepository.SearchAsync(locationsQuery, cancellationToken);

            Guid? currentUserId = null;
            if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
            {
                var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
                if (Guid.TryParse(userClaim, out Guid parsedUserId))
                {
                    currentUserId = parsedUserId;
                }
            }

            foreach (var contractLocation in contractLocations)
            {
                var existingLinkQuery = contractFoodLocationRepository
                    .MultipleResultQuery()
                    .AndFilter(x => !x.Disabled && !x.hidden && x.ContractFoodId == updated.Id && x.ContractLocationId == contractLocation.Id);
                var existingLinks = await contractFoodLocationRepository.SearchAsync(existingLinkQuery, cancellationToken);
                if (existingLinks.Any())
                {
                    continue;
                }

                var link = new Models.ContractFoodLocation
                {
                    Id = Guid.NewGuid(),
                    ContractFoodId = updated.Id,
                    ContractLocationId = contractLocation.Id,
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

        return ResultDTO<ContractFoodDTO>.Success(
            _mapper.Map<ContractFoodDTO>(updated),
            "رابطه غذا-قرارداد با موفقیت بروزرسانی شد"
        );
    }
}


