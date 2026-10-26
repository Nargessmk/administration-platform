using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.OrganizationalPosition.Query;

public sealed class GetOrganizationalPositionByIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.OrganizationalPosition> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.OrganizationalPosition, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetOrganizationalPositionByIdQuery, ResultDTO<OrganizationalPositionDTO>>
{
    public async Task<ResultDTO<OrganizationalPositionDTO>> Handle(GetOrganizationalPositionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext 
                ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            var organizationalPosition = await _dbContext.OrganizationalPositions
                .Where(x => x.Id == request.Id)
                .Where(x => !x.Disabled)
                .Where(x => !x.hidden)   
                .FirstOrDefaultAsync(cancellationToken);

            if (organizationalPosition == null)
            {
                return ResultDTO<OrganizationalPositionDTO>.Failure(
                    "سمت سازمانی مورد نظر یافت نشد ",
                    []
                );
            }

            return ResultDTO<OrganizationalPositionDTO>.Success(
                _mapper.Map<OrganizationalPositionDTO>(organizationalPosition),
                "سمت سازمانی با موفقیت برگشت داده شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<OrganizationalPositionDTO>.Failure(
                $"خطا در دریافت سمت سازمانی: {ex.Message}",
                []
            );
        }
    }
}
