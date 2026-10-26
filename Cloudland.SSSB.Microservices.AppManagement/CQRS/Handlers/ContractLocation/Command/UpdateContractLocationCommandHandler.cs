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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractLocation.Command;

public sealed class UpdateContractLocationCommandHandler(
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
        IRequestHandler<UpdateContractLocationCommand, ResultDTO<ContractLocationDTO>?>
{
    public async Task<ResultDTO<ContractLocationDTO>?> Handle(UpdateContractLocationCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }

        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.ContractLocation.Id);
        var existing = await _repository.SingleOrDefaultAsync(query, cancellationToken);

        if (existing == null)
        {
            return ResultDTO<ContractLocationDTO>.Failure(
                "رابطه مکان-قرارداد پیدا نشد",
                []
            );
        }

        if (!existing.AllowUpdate)
        {
            return ResultDTO<ContractLocationDTO>.Failure(
                "رابطه مکان-قرارداد قابل بروزرسانی نمی باشد",
                request.ContractLocation
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

        existing.ContractId = request.ContractLocation.ContractId;
        existing.LocationId = request.ContractLocation.LocationId;

        var updated = _repository.Update(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Ensure links exist to all foods of the same contract
        if (updated.ContractId.HasValue)
        {
            var foodsQuery = contractFoodRepository
                .MultipleResultQuery()
                .AndFilter(x => !x.Disabled && !x.hidden && x.ContractId == updated.ContractId);
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
                    .AndFilter(x => !x.Disabled && !x.hidden && x.ContractFoodId == contractFood.Id && x.ContractLocationId == updated.Id);
                var existingLinks = await contractFoodLocationRepository.SearchAsync(existingLinkQuery, cancellationToken);
                if (existingLinks.Any())
                {
                    continue;
                }

                var link = new Models.ContractFoodLocation
                {
                    Id = Guid.NewGuid(),
                    ContractFoodId = contractFood.Id,
                    ContractLocationId = updated.Id,
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
            _mapper.Map<ContractLocationDTO>(updated),
            "رابطه مکان-قرارداد با موفقیت بروزرسانی شد"
        );
    }
}


