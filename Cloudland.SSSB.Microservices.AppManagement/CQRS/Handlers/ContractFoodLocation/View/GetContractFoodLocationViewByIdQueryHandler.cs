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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFoodLocation.View;

public sealed class GetContractFoodLocationViewByIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFoodLocation> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ContractFoodLocationView> contractFoodLocationViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractFoodLocation, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractFoodLocationViewByIdQuery, ResultDTO<ContractFoodLocationViewDTO>>
{
    public async Task<ResultDTO<ContractFoodLocationViewDTO>> Handle(
        GetContractFoodLocationViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = contractFoodLocationViewRepository.MultipleResultQuery()
                .AndFilter(x => x.Id == request.Id);
            var entities = await contractFoodLocationViewRepository.SearchAsync(query, cancellationToken);
            var entity = entities.FirstOrDefault();
            if (entity == null)
            {
                return ResultDTO<ContractFoodLocationViewDTO>.Failure("رکورد مورد نظر یافت نشد", []);
            }
            return ResultDTO<ContractFoodLocationViewDTO>.Success(_mapper.Map<ContractFoodLocationViewDTO>(entity), "آیتم با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            return ResultDTO<ContractFoodLocationViewDTO>.Failure($"خطا در دریافت آیتم: {ex.Message}", []);
        }
    }
}


