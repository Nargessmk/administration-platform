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

public sealed class GetContractViewAllQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Contract> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ContractView> contractViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Contract, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractViewAllQuery, ResultDTO<List<ContractViewDTO>>>
{
    public async Task<ResultDTO<List<ContractViewDTO>>> Handle(
        GetContractViewAllQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = contractViewRepository.MultipleResultQuery();

            var entities = await contractViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<ContractViewDTO>>(entities);

            return ResultDTO<List<ContractViewDTO>>.Success(
                dtos,
                "لیست کامل قراردادها با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<ContractViewDTO>>.Failure(
                $"خطا در دریافت لیست قراردادها: {ex.Message}",
                new List<ContractViewDTO>()
            );
        }
    }
}
