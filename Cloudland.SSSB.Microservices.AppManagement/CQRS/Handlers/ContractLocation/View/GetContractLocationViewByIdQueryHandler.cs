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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractLocation.View;

public sealed class GetContractLocationViewByIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractLocation> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ContractLocationView> contractLocationViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractLocation, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractLocationViewByIdQuery, ResultDTO<ContractLocationViewDTO>>
{
    public async Task<ResultDTO<ContractLocationViewDTO>> Handle(
        GetContractLocationViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = contractLocationViewRepository
                .SingleResultQuery()
                .AndFilter(x => x.Id == request.Id);

            var entity = await contractLocationViewRepository.SingleOrDefaultAsync(query, cancellationToken);

            if (entity is null)
            {
                return ResultDTO<ContractLocationViewDTO>.Failure(
                    "رکورد مورد نظر یافت نشد",
                    []
                );
            }

            var dto = _mapper.Map<ContractLocationViewDTO>(entity);

            return ResultDTO<ContractLocationViewDTO>.Success(
                dto,
                "با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<ContractLocationViewDTO>.Failure(
                $"خطا در دریافت: {ex.Message}",
                []
            );
        }
    }
}



