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

public sealed class GetOrganizationalPositionViewByIdQueryHandler(
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
        IRequestHandler<GetOrganizationalPositionViewByIdQuery, ResultDTO<OrganizationalPositionViewDTO>>
{
    public async Task<ResultDTO<OrganizationalPositionViewDTO>> Handle(
        GetOrganizationalPositionViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = organizationalPositionViewRepository
                .SingleResultQuery()
                .AndFilter(x => x.Id == request.Id);

            var entity = await organizationalPositionViewRepository.SingleOrDefaultAsync(query, cancellationToken);

            if (entity is null)
            {
                return ResultDTO<OrganizationalPositionViewDTO>.Failure(
                    "سمت سازمانی مورد نظر یافت نشد",
                    []
                );
            }

            var dto = _mapper.Map<OrganizationalPositionViewDTO>(entity);

            return ResultDTO<OrganizationalPositionViewDTO>.Success(
                dto,
                "سمت سازمانی با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<OrganizationalPositionViewDTO>.Failure(
                $"خطا در دریافت سمت سازمانی: {ex.Message}",
                []
            );
        }
    }
}
