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

public sealed class GetContractFoodLocationViewAllByLocationIdQueryHandler(
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
        IRequestHandler<GetContractFoodLocationViewAllByLocationIdQuery, ResultDTO<List<ContractFoodLocationViewDTO>>>
{
    public async Task<ResultDTO<List<ContractFoodLocationViewDTO>>> Handle(
        GetContractFoodLocationViewAllByLocationIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = contractFoodLocationViewRepository.MultipleResultQuery()
                .AndFilter(x => x.LocationId == request.LocationId);
            var entities = await contractFoodLocationViewRepository.SearchAsync(query, cancellationToken);
            var dtos = entities.Select(x => _mapper.Map<ContractFoodLocationViewDTO>(x)).ToList();
            return ResultDTO<List<ContractFoodLocationViewDTO>>.Success(dtos, "لیست با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            return ResultDTO<List<ContractFoodLocationViewDTO>>.Failure($"خطا در دریافت لیست: {ex.Message}", [""]);
        }
    }
}


