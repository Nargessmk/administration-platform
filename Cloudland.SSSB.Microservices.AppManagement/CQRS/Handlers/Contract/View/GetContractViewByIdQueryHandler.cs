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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Contract.View;

public sealed class GetContractViewByIdQueryHandler(
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
        IRequestHandler<GetContractViewByIdQuery, ResultDTO<ContractViewDTO>>
{
    public async Task<ResultDTO<ContractViewDTO>> Handle(
        GetContractViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = contractViewRepository
                .SingleResultQuery()
                .AndFilter(x => x.Id == request.Id);

            var entity = await contractViewRepository.SingleOrDefaultAsync(query, cancellationToken);

            if (entity is null)
            {
                return ResultDTO<ContractViewDTO>.Failure(
                    "قرارداد مورد نظر یافت نشد",
                    []
                );
            }

            var dto = _mapper.Map<ContractViewDTO>(entity);

            return ResultDTO<ContractViewDTO>.Success(
                dto,
                "قرارداد با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<ContractViewDTO>.Failure(
                $"خطا در دریافت قرارداد: {ex.Message}",
                []
            );
        }
    }
}



