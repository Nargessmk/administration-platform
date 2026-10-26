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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.AccountNumber.Command;

public sealed class DeleteAccountNumberCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.AccountNumber> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<DeleteAccountNumberCommandHandler> logger)
    : BaseHandler<Models.AccountNumber, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteAccountNumberCommand, ResultDTO<int>?>
{


    public async Task<ResultDTO<int>?> Handle(DeleteAccountNumberCommand request, CancellationToken cancellationToken)
    {

            // Check if entity exists
            var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
            var accountNumberToDelete = await _repository.SingleOrDefaultAsync(query, cancellationToken);
            if (accountNumberToDelete == null)
            {
                return ResultDTO<int>.Failure(
                    $"شماره حسابی با این شناسه برای حذف پیدا نشد",
                    0
                ); // No rows affected
            }
            
            if (!accountNumberToDelete.AllowDelete)
            {
                return ResultDTO<int>.Failure(
                    "شماره حساب قابل حذف نمی باشد",
                    null
                ); // No rows affected
            }
        
            if (accountNumberToDelete.Disabled)
            {
                logger.LogInformation($"AccountNumber with ID {request.Id} is disabled, proceeding with deletion");
            }

            var affectedRows = await _repository.RemoveAsync(x => x.Id == request.Id, cancellationToken);
        
            if (affectedRows > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
            }
            else
            {
                logger.LogWarning($"No rows were affected when deleting AccountNumber with ID {request.Id}");
            }

            return ResultDTO<int>.Success(
                affectedRows,
            
                $"شماره حساب {accountNumberToDelete.title} با موفقیت حذف شد"
            );

    }
}
