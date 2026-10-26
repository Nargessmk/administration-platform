using AutoMapper;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFood.Query;

public sealed class GetContractFoodByIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFood> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractFood, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractFoodByIdQuery, ResultDTO<ContractFoodDTO>>
{
    public async Task<ResultDTO<ContractFoodDTO>> Handle(GetContractFoodByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext 
                ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            var entity = await _dbContext.ContractFoods
                .Where(x => x.Id == request.Id)
                .Where(x => !x.Disabled)
                .Where(x => !x.hidden)
                .FirstOrDefaultAsync(cancellationToken);

            if (entity == null)
            {
                return ResultDTO<ContractFoodDTO>.Failure(
                    "رابطه غذا-قرارداد مورد نظر یافت نشد ",
                    []
                );
            }

            return ResultDTO<ContractFoodDTO>.Success(
                _mapper.Map<ContractFoodDTO>(entity),
                "رابطه غذا-قرارداد با موفقیت برگشت داده شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<ContractFoodDTO>.Failure(
                $"خطا در دریافت رابطه غذا-قرارداد: {ex.Message}",
                []
            );
        }
    }
}


