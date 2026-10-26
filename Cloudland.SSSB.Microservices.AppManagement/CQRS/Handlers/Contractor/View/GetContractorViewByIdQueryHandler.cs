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

public sealed class GetContractorViewByIdQueryHandler(
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
        IRequestHandler<GetContractorViewByIdQuery, ResultDTO<ContractorViewDTO>>
{
    public async Task<ResultDTO<ContractorViewDTO>> Handle(
        GetContractorViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = contractorViewRepository
                .SingleResultQuery()
                .AndFilter(x => x.Id == request.Id && !x.Disabled && !x.hidden);

            var entity = await contractorViewRepository.SingleOrDefaultAsync(query, cancellationToken);

            if (entity is null)
            {
                return ResultDTO<ContractorViewDTO>.Failure(
                    "پیمانکار مورد نظر یافت نشد",
                    []
                );
            }

            var dto = _mapper.Map<ContractorViewDTO>(entity);

            return ResultDTO<ContractorViewDTO>.Success(
                dto,
                "پیمانکار با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<ContractorViewDTO>.Failure(
                $"خطا در دریافت اطلاعات پیمانکار: {ex.Message}",
                []
            );
        }
    }
}


