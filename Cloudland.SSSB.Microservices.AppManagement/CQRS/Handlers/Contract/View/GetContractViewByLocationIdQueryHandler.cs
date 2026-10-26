using AutoMapper;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Contract.View;

public sealed class GetContractViewByLocationIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Contract> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ContractView> contractViewRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ContractLocationView> contractLocationViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Contract, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractViewByLocationIdQuery, ResultDTO<List<ContractViewDTO>>>
{
    public async Task<ResultDTO<List<ContractViewDTO>>> Handle(
        GetContractViewByLocationIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var clQuery = contractLocationViewRepository
                .MultipleResultQuery()
                .AndFilter(x => x.LocationId == request.LocationId);
            var contractLocations = await contractLocationViewRepository.SearchAsync(clQuery, cancellationToken);
            var contractIds = contractLocations.Select(x => x.ContractId).Distinct().ToList();

            if (contractIds.Count == 0)
            {
                return ResultDTO<List<ContractViewDTO>>.Success(new List<ContractViewDTO>(), "قراردادی برای این مکان یافت نشد");
            }

            var contractQuery = contractViewRepository
                .MultipleResultQuery()
                .AndFilter(v => contractIds.Contains(v.Id));
            var contractViews = await contractViewRepository.SearchAsync(contractQuery, cancellationToken);

            var dtos = _mapper.Map<List<ContractViewDTO>>(contractViews);
            return ResultDTO<List<ContractViewDTO>>.Success(dtos, "لیست قراردادهای این مکان با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            return ResultDTO<List<ContractViewDTO>>.Failure(
                $"خطا در دریافت قراردادها برای مکان: {ex.Message}",
                new List<ContractViewDTO>()
            );
        }
    }
}
