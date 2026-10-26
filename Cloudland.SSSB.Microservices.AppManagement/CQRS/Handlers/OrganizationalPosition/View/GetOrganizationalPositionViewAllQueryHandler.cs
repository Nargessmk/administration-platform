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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.OrganizationalPosition.View;

public sealed class GetOrganizationalPositionViewAllQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.OrganizationalPosition> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<OrganizationalPositionView> organizationalPositionViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.OrganizationalPosition, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetOrganizationalPositionViewAllQuery, ResultDTO<List<OrganizationalPositionViewDTO>>>
{
    public async Task<ResultDTO<List<OrganizationalPositionViewDTO>>> Handle(
        GetOrganizationalPositionViewAllQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = organizationalPositionViewRepository.MultipleResultQuery();
            var entities = await organizationalPositionViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<OrganizationalPositionViewDTO>>(entities);

            return ResultDTO<List<OrganizationalPositionViewDTO>>.Success(
                dtos,
                "لیست کامل سمت‌های سازمانی با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<OrganizationalPositionViewDTO>>.Failure(
                $"خطا در دریافت لیست سمت‌های سازمانی: {ex.Message}",
                new List<OrganizationalPositionViewDTO>()
            );
        }
    }
}
