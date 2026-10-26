using AutoMapper;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using Cloudland.SSSB.Models;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Organizational_Charts.Query;

public sealed class GetOrganizationalChartPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<OrganizationalChart> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<OrganizationalChart, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetOrganizationalChartPagingQuery, ResultDTO<PaginatedResult<OrganizationalChartDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<OrganizationalChartDTO>>> Handle(GetOrganizationalChartPagingQuery request, CancellationToken cancellationToken)
    {
        
        try
        {
            // Get the database context
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext 
                             ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");
            
            var query = _dbContext.OrganizationalCharts
                .Where(x => !x.Disabled && !x.hidden)
                .OrderBy(x => x.title).AsQueryable();

            //pagination
            var paginatedResult = new PaginatedResult<OrganizationalChartDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.Paginate(query.Select(x => _mapper.Map<OrganizationalChartDTO>(x)));

            return ResultDTO<PaginatedResult<OrganizationalChartDTO>>.Success(
                paginatedResult,
                "لیست سازمان با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<OrganizationalChartDTO>>.Failure(
                $"خطا در دریافت لیست سازمان ها: {ex.Message}",
                []
            );
        }
    }
}
