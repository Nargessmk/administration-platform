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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractLocation.View;

public sealed class GetContractLocationViewAllByContractIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractLocation> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractLocation, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractLocationViewAllByContractIdQuery, ResultDTO<List<ContractLocationViewDTO>>>
{
    public async Task<ResultDTO<List<ContractLocationViewDTO>>> Handle(
        GetContractLocationViewAllByContractIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext
                             ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            var entities = await _dbContext.Set<ContractLocationView>()
                .Where(x => x.ContractId == request.ContractId)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<ContractLocationViewDTO>>(entities);

            return ResultDTO<List<ContractLocationViewDTO>>.Success(
                dtos,
                "لیست مکان‌های قرارداد با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<ContractLocationViewDTO>>.Failure(
                $"خطا در دریافت لیست مکان‌های قرارداد: {ex.Message}",
                new List<ContractLocationViewDTO>()
            );
        }
    }
}



