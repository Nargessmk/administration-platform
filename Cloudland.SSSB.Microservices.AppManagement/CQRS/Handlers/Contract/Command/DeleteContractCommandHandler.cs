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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Contract.Command;

public sealed class DeleteContractCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Contract> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFood> contractFoodRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractLocation> contractLocationRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFoodLocation> contractFoodLocationRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<DeleteContractCommandHandler> logger)
    : BaseHandler<Models.Contract, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteContractCommand, ResultDTO<int>?>
{
    public async Task<ResultDTO<int>?> Handle(DeleteContractCommand request, CancellationToken cancellationToken)
    {
        // Validate target contract
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
        var rootContract = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        if (rootContract == null)
        {
            return ResultDTO<int>.Failure(
                "قراردادی با این شناسه برای حذف پیدا نشد",
                0
            );
        }

        if (!rootContract.AllowDelete)
        {
            return ResultDTO<int>.Failure(
                "قرارداد قابل حذف نمی باشد",
                null
            );
        }

        // Collect all descendant contract IDs (BFS) including the root
        var idsToDelete = new HashSet<Guid> { request.Id };
        var frontier = new List<Guid> { request.Id };
        while (frontier.Count > 0)
        {
            var currentLevel = frontier.ToList();
            frontier.Clear();

            var childrenQuery = _repository
                .MultipleResultQuery()
                .AndFilter(c => c.ParentId != null && currentLevel.Contains(c.ParentId.Value));
            var children = await _repository.SearchAsync(childrenQuery, cancellationToken);

            foreach (var child in children)
            {
                if (idsToDelete.Add(child.Id))
                {
                    frontier.Add(child.Id);
                }
            }
        }

        // Delete dependents in correct order due to FK constraints (NoAction)
        int affected = 0;

        if (idsToDelete.Count > 0)
        {
            // Load ContractFood and ContractLocation IDs for those contracts
            var contractFoodsQuery = contractFoodRepository
                .MultipleResultQuery()
                .AndFilter(cf => cf.ContractId != null && idsToDelete.Contains(cf.ContractId.Value));
            var foods = await contractFoodRepository.SearchAsync(contractFoodsQuery, cancellationToken);
            var foodIds = foods.Select(f => f.Id).ToList();

            var contractLocationsQuery = contractLocationRepository
                .MultipleResultQuery()
                .AndFilter(cl => cl.ContractId != null && idsToDelete.Contains(cl.ContractId.Value));
            var locations = await contractLocationRepository.SearchAsync(contractLocationsQuery, cancellationToken);
            var locationIds = locations.Select(l => l.Id).ToList();

            // Delete join table first: ContractFoodLocation
            if (foodIds.Count > 0 || locationIds.Count > 0)
            {
                affected += await contractFoodLocationRepository.RemoveAsync(
                    x => (x.ContractFoodId != null && foodIds.Contains(x.ContractFoodId.Value))
                         || (x.ContractLocationId != null && locationIds.Contains(x.ContractLocationId.Value)),
                    cancellationToken);
            }

            // Delete children tables
            if (foodIds.Count > 0)
            {
                affected += await contractFoodRepository.RemoveAsync(x => foodIds.Contains(x.Id), cancellationToken);
            }

            if (locationIds.Count > 0)
            {
                affected += await contractLocationRepository.RemoveAsync(x => locationIds.Contains(x.Id), cancellationToken);
            }

            // Finally delete contracts (root + descendants)
            affected += await _repository.RemoveAsync(x => idsToDelete.Contains(x.Id), cancellationToken);
        }

        if (affected > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        }
        else
        {
            logger.LogWarning($"No rows were affected when deleting Contract (cascade) with root ID {request.Id}");
        }

        return ResultDTO<int>.Success(
            affected,
            "حذف قرارداد و وابستگی‌ها با موفقیت انجام شد"
        );
    }
}


