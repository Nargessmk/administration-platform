using AutoMapper;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFood.View;

public sealed class GetContractFoodViewAllByContractIdQueryHandler(
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
        IRequestHandler<GetContractFoodViewAllByContractIdQuery, ResultDTO<List<ContractFoodViewDTO>>>
{
    public async Task<ResultDTO<List<ContractFoodViewDTO>>> Handle(
        GetContractFoodViewAllByContractIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext
                             ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            var entities = await _dbContext.Set<ContractFoodView>()
                .Where(x => x.ContractId == request.ContractId)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<ContractFoodViewDTO>>(entities);

            return ResultDTO<List<ContractFoodViewDTO>>.Success(
                dtos,
                "لیست غذاهای قرارداد با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<ContractFoodViewDTO>>.Failure(
                $"خطا در دریافت لیست: {ex.Message}",
                new List<ContractFoodViewDTO>()
            );
        }
    }
}



