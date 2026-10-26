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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFoodLocation.Command;

public sealed class UpdateContractFoodLocationCommandHandler(
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
        IRequestHandler<UpdateContractFoodLocationCommand, ResultDTO<ContractFoodLocationDTO>?>
{
    public async Task<ResultDTO<ContractFoodLocationDTO>?> Handle(UpdateContractFoodLocationCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }

        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.ContractFoodLocation.Id);
        var existing = await _repository.SingleOrDefaultAsync(query, cancellationToken);

        if (existing == null)
        {
            return ResultDTO<ContractFoodLocationDTO>.Failure(
                "رابطه غذای قرارداد-مکان پیدا نشد",
                []
            );
        }

        if (!existing.AllowUpdate)
        {
            return ResultDTO<ContractFoodLocationDTO>.Failure(
                "رابطه غذای قرارداد-مکان قابل بروزرسانی نمی باشد",
                request.ContractFoodLocation
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

        existing.ContractFoodId = request.ContractFoodLocation.ContractFoodId;
        existing.ContractLocationId = request.ContractFoodLocation.ContractLocationId;

        var updated = _repository.Update(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        return ResultDTO<ContractFoodLocationDTO>.Success(
            _mapper.Map<ContractFoodLocationDTO>(updated),
            "رابطه غذای قرارداد-مکان با موفقیت بروزرسانی شد"
        );
    }
}
