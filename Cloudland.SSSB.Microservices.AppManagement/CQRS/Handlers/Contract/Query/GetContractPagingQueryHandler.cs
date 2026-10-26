using AutoMapper;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Contract.Query;

public sealed class GetContractPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Contract> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Contract, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractPagingQuery, ResultDTO<PaginatedResult<ContractDTO>>>
{

    public async Task<ResultDTO<PaginatedResult<ContractDTO>>> Handle(GetContractPagingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext 
                             ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            var query = _dbContext.Contracts
                .Where(x => !x.Disabled && !x.hidden)
                .OrderBy(x => x.ContractNumber)
                .AsQueryable();

            var paginatedResult = new PaginatedResult<ContractDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.Paginate(query.Select(x => _mapper.Map<ContractDTO>(x)));

            return ResultDTO<PaginatedResult<ContractDTO>>.Success(
                paginatedResult,
                "لیست قراردادها با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ContractDTO>>.Failure(
                $"خطا در دریافت لیست قراردادها: {ex.Message}",
                []
            );
        }
    }
}


