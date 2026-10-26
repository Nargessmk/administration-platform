using AutoMapper;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Models;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Organizational_Charts.View;

public sealed class GetOrganizationalChartViewByIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.OrganizationalChart> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<OrganizationalChartView> organizationalChartViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<OrganizationalChart, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetOrganizationalChartViewByIdQuery, ResultDTO<OrganizationalChartViewDTO>>
{
    public async Task<ResultDTO<OrganizationalChartViewDTO>> Handle(
        GetOrganizationalChartViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = organizationalChartViewRepository
                .SingleResultQuery()
                .AndFilter(x => x.Id == request.Id && !x.Disabled && !x.hidden);

            var entity = await organizationalChartViewRepository.SingleOrDefaultAsync(query, cancellationToken);

            if (entity is null)
            {
                return ResultDTO<OrganizationalChartViewDTO>.Failure(
                    "سازمان مورد نظر یافت نشد",
                    []
                );
            }

            var dto = _mapper.Map<OrganizationalChartViewDTO>(entity);

            return ResultDTO<OrganizationalChartViewDTO>.Success(
                dto,
                "سازمان با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<OrganizationalChartViewDTO>.Failure(
                $"خطا در دریافت اطلاعات سازمان: {ex.Message}",
                []
            );
        }
    }
}
