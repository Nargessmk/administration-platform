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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFoodLocation.Query;

public sealed class GetContractFoodLocationByIdQueryHandler(
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
        IRequestHandler<GetContractFoodLocationByIdQuery, ResultDTO<ContractFoodLocationDTO>>
{
    public async Task<ResultDTO<ContractFoodLocationDTO>> Handle(GetContractFoodLocationByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext 
                ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            var entity = await _dbContext.ContractFoodLocations
                .Where(x => x.Id == request.Id)
                .Where(x => !x.Disabled)
                .Where(x => !x.hidden)
                .FirstOrDefaultAsync(cancellationToken);

            if (entity == null)
            {
                return ResultDTO<ContractFoodLocationDTO>.Failure(
                    "رابطه غذای قرارداد-مکان مورد نظر یافت نشد ",
                    []
                );
            }

            return ResultDTO<ContractFoodLocationDTO>.Success(
                _mapper.Map<ContractFoodLocationDTO>(entity),
                "رابطه غذای قرارداد-مکان با موفقیت برگشت داده شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<ContractFoodLocationDTO>.Failure(
                $"خطا در دریافت رابطه غذای قرارداد-مکان: {ex.Message}",
                []
            );
        }
    }
}
