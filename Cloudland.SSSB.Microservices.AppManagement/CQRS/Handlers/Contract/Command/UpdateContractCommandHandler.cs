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

public sealed class UpdateContractCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Contract> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Contract, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<UpdateContractCommand, ResultDTO<ContractDTO>?>
{
    public async Task<ResultDTO<ContractDTO>?> Handle(UpdateContractCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }

        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Contract.Id);
        var existing = await _repository.SingleOrDefaultAsync(query, cancellationToken);

        if (existing == null)
        {
            return ResultDTO<ContractDTO>.Failure(
                "قرارداد پیدا نشد",
                []
            );
        }

        if (!existing.AllowUpdate)
        {
            return ResultDTO<ContractDTO>.Failure(
                "قرارداد قابل بروزرسانی نمی باشد",
                request.Contract
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

        existing.ContractorId = request.Contract.ContractorId;
        existing.StartDate = _mapper.Map<DateTime?>(request.Contract.StartDate);
        existing.EndDate = _mapper.Map<DateTime?>(request.Contract.EndDate);
        existing.ContractNumber = request.Contract.ContractNumber;
        existing.IsActive = request.Contract.IsActive;
        existing.Quantity = request.Contract.Quantity;
        existing.Price = request.Contract.Price;
        existing.AdjustmentPercentage = request.Contract.AdjustmentPercentage;
        existing.ContractTypeId = request.Contract.ContractTypeId;


        var updated = _repository.Update(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        return ResultDTO<ContractDTO>.Success(
            _mapper.Map<ContractDTO>(updated),
            "قرارداد با موفقیت بروزرسانی شد"
        );
    }
}


