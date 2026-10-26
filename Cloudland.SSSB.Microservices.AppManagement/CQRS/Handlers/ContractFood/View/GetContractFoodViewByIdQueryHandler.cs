using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFood.View;

public sealed class GetContractFoodViewByIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFood> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ContractFoodView> contractFoodViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractFood, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractFoodViewByIdQuery, ResultDTO<ContractFoodViewDTO>>
{
    public async Task<ResultDTO<ContractFoodViewDTO>> Handle(
        GetContractFoodViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = contractFoodViewRepository
                .SingleResultQuery()
                .AndFilter(x => x.Id == request.Id);

            var entity = await contractFoodViewRepository.SingleOrDefaultAsync(query, cancellationToken);

            if (entity is null)
            {
                return ResultDTO<ContractFoodViewDTO>.Failure(
                    "رکورد مورد نظر یافت نشد",
                    []
                );
            }

            var dto = _mapper.Map<ContractFoodViewDTO>(entity);

            return ResultDTO<ContractFoodViewDTO>.Success(
                dto,
                "با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<ContractFoodViewDTO>.Failure(
                $"خطا در دریافت: {ex.Message}",
                []
            );
        }
    }
}



