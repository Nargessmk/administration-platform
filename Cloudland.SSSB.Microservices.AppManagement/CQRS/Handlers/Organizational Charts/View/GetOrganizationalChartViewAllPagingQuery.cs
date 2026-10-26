using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using Cloudland.SSSB.Models;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Organizational_Charts.View;

public sealed class GetOrganizationalChartViewAllPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<OrganizationalChart> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<OrganizationalChartView> organizationalChartViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<OrganizationalChart, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetOrganizationalChartViewAllPagingQuery, ResultDTO<PaginatedResult<OrganizationalChartViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<OrganizationalChartViewDTO>>> Handle(
        GetOrganizationalChartViewAllPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = organizationalChartViewRepository.MultipleResultQuery();
            var entities = await organizationalChartViewRepository.SearchAsync(query, cancellationToken);
            var mappedEntities = entities.Select(x => _mapper.Map<OrganizationalChartViewDTO>(x));

            var paginatedResult = new PaginatedResult<OrganizationalChartViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(mappedEntities);

            return ResultDTO<PaginatedResult<OrganizationalChartViewDTO>>.Success(
                paginatedResult,
                "لیست سازمان‌ها با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<OrganizationalChartViewDTO>>.Failure(
                $"خطا در دریافت لیست سازمان‌ها: {ex.Message}",
                []
            );
        }
    }
}
