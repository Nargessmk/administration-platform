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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractLocation.Query;

public sealed class GetContractLocationByIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractLocation> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractLocation, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractLocationByIdQuery, ResultDTO<ContractLocationDTO>>
{
    public async Task<ResultDTO<ContractLocationDTO>> Handle(GetContractLocationByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext 
                ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            var entity = await _dbContext.ContractLocations
                .Where(x => x.Id == request.Id)
                .Where(x => !x.Disabled)
                .Where(x => !x.hidden)
                .FirstOrDefaultAsync(cancellationToken);

            if (entity == null)
            {
                return ResultDTO<ContractLocationDTO>.Failure(
                    "رابطه مکان-قرارداد مورد نظر یافت نشد ",
                    []
                );
            }

            return ResultDTO<ContractLocationDTO>.Success(
                _mapper.Map<ContractLocationDTO>(entity),
                "رابطه مکان-قرارداد با موفقیت برگشت داده شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<ContractLocationDTO>.Failure(
                $"خطا در دریافت رابطه مکان-قرارداد: {ex.Message}",
                []
            );
        }
    }
}


