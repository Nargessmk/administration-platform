using AutoMapper;
using Cloudland.APM.DTO;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Models;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Organizational_Charts.View;

public sealed class GetOrganizationalChartRecursiveAllQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.OrganizationalChart> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<OrganizationalChart, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetOrganizationalChartRecursiveAllQuery, ResultDTO<List<ConstantRecursiveDTO>>>
{
    public async Task<ResultDTO<List<ConstantRecursiveDTO>>> Handle(
        GetOrganizationalChartRecursiveAllQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext
                             ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            // Call the recursive 
            var nodes = await _dbContext
                .GetOrganizationalChartRecursive(request.RootId)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<ConstantRecursiveDTO>>(nodes);

            return ResultDTO<List<ConstantRecursiveDTO>>.Success(
                dtos,
                "زیرساخت سازمان با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<ConstantRecursiveDTO>>.Failure(
                $"خطا در دریافت زیرساخت سازمان: {ex.Message}",
                new List<ConstantRecursiveDTO>()
            );
        }
    }
}
