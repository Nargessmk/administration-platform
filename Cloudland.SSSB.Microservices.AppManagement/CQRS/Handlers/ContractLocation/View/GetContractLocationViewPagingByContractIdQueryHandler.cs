using AutoMapper;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractLocation.View;

public sealed class GetContractLocationViewPagingByContractIdQueryHandler(
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
        IRequestHandler<GetContractLocationViewPagingByContractIdQuery, ResultDTO<PaginatedResult<ContractLocationViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<ContractLocationViewDTO>>> Handle(
        GetContractLocationViewPagingByContractIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext
                             ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            var query = _dbContext.Set<ContractLocationView>()
                .Where(x => x.ContractId == request.ContractId)
                .AsQueryable();

            var paginatedResult = new PaginatedResult<ContractLocationViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.Paginate(query.Select(x => _mapper.Map<ContractLocationViewDTO>(x)));

            return ResultDTO<PaginatedResult<ContractLocationViewDTO>>.Success(
                paginatedResult,
                "لیست با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ContractLocationViewDTO>>.Failure(
                $"خطا در دریافت لیست: {ex.Message}",
                []
            );
        }
    }
}



