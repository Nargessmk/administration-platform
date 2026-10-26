using AutoMapper;
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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.OrganizationalPosition.View;

public sealed class GetOrganizationalPositionViewChildrenByParentIdPagingQueryHandler(
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
        IRequestHandler<GetOrganizationalPositionViewChildrenByParentIdPagingQuery, ResultDTO<PaginatedResult<OrganizationalPositionViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<OrganizationalPositionViewDTO>>> Handle(
        GetOrganizationalPositionViewChildrenByParentIdPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = organizationalPositionViewRepository
                .MultipleResultQuery()
                .AndFilter(x => x.ParentId == request.ParentId)
                .OrderByDescending(x => x.CreateAt);

            var entities = await organizationalPositionViewRepository.SearchAsync(query, cancellationToken);
            var mappedEntities = entities.Select(x => _mapper.Map<OrganizationalPositionViewDTO>(x));

            var paginatedResult = new PaginatedResult<OrganizationalPositionViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(mappedEntities);

            return ResultDTO<PaginatedResult<OrganizationalPositionViewDTO>>.Success(
                paginatedResult,
                "لیست فرزندان سمت سازمانی با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<OrganizationalPositionViewDTO>>.Failure(
                $"خطا در دریافت لیست فرزندان سمت سازمانی: {ex.Message}",
                []
            );
        }
    }
}
