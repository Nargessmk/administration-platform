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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Contract.Query;

public sealed class GetContractByIdQueryHandler(
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
        IRequestHandler<GetContractByIdQuery, ResultDTO<ContractDTO>>
{
    public async Task<ResultDTO<ContractDTO>> Handle(GetContractByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext 
                ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            var entity = await _dbContext.Contracts
                .Where(x => x.Id == request.Id)
                .Where(x => !x.Disabled)
                .Where(x => !x.hidden)
                .FirstOrDefaultAsync(cancellationToken);

            if (entity == null)
            {
                return ResultDTO<ContractDTO>.Failure(
                    "قرارداد مورد نظر یافت نشد ",
                    []
                );
            }

            return ResultDTO<ContractDTO>.Success(
                _mapper.Map<ContractDTO>(entity),
                "قرارداد با موفقیت برگشت داده شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<ContractDTO>.Failure(
                $"خطا در دریافت قرارداد: {ex.Message}",
                []
            );
        }
    }
}


