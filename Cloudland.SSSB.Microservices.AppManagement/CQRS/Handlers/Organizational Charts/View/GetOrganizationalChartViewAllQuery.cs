using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Models;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Organizational_Charts.View;

public sealed class GetOrganizationalChartViewAllQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<OrganizationalChart> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<OrganizationalChartView> organizationalChartViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<OrganizationalChart, Guid>(unitOfWork,repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetOrganizationalChartViewAllQuery, ResultDTO<List<OrganizationalChartViewDTO>>>
{
    public async Task<ResultDTO<List<OrganizationalChartViewDTO>>> Handle(
        GetOrganizationalChartViewAllQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = organizationalChartViewRepository.MultipleResultQuery();
            var entities = await organizationalChartViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<OrganizationalChartViewDTO>>(entities);

            return ResultDTO<List<OrganizationalChartViewDTO>>.Success(
                dtos,
                "لیست کامل سازمان‌ها با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<OrganizationalChartViewDTO>>.Failure(
                $"خطا در دریافت لیست سازمان‌ها: {ex.Message}",
                new List<OrganizationalChartViewDTO>()
            );
        }
    }
}