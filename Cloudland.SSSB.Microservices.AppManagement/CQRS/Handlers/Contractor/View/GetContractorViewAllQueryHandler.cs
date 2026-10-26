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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Contractor.View;

public sealed class GetContractorViewAllQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Contractor> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ContractorView> contractorViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Contractor, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractorViewAllQuery, ResultDTO<List<ContractorViewDTO>>>
{
    public async Task<ResultDTO<List<ContractorViewDTO>>> Handle(
        GetContractorViewAllQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = contractorViewRepository.MultipleResultQuery();

            var entities = await contractorViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<ContractorViewDTO>>(entities);

            return ResultDTO<List<ContractorViewDTO>>.Success(
                dtos,
                "لیست کامل پیمانکاران با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<ContractorViewDTO>>.Failure(
                $"خطا در دریافت لیست پیمانکاران: {ex.Message}",
                new List<ContractorViewDTO>()
            );
        }
    }
}


