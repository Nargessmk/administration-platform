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

public sealed class DeleteContractLocationCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractLocation> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFoodLocation> contractFoodLocationRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<DeleteContractLocationCommandHandler> logger)
    : BaseHandler<Models.ContractLocation, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteContractLocationCommand, ResultDTO<int>?>
{
    public async Task<ResultDTO<int>?> Handle(DeleteContractLocationCommand request, CancellationToken cancellationToken)
    {
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
        var toDelete = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        if (toDelete == null)
        {
            return ResultDTO<int>.Failure(
                "رابطه مکان-قرارداد با این شناسه برای حذف پیدا نشد",
                0
            );
        }

        if (!toDelete.AllowDelete)
        {
            return ResultDTO<int>.Failure(
                "این رکورد قابل حذف نمی باشد",
                null
            );
        }

        // Remove dependent join rows first
        var removedLinks = await contractFoodLocationRepository.RemoveAsync(x => x.ContractLocationId == request.Id, cancellationToken);

        var affectedRows = await _repository.RemoveAsync(x => x.Id == request.Id, cancellationToken);

        if (affectedRows > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        }
        else
        {
            logger.LogWarning($"No rows were affected when deleting contract-location with ID {request.Id}");
        }

        return ResultDTO<int>.Success(
            affectedRows,
            "رابطه مکان-قرارداد با موفقیت حذف شد"
        );
    }
}


