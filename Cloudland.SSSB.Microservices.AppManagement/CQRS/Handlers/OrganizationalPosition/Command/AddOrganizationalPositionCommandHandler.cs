using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;
using Cloudland.SSSB.Models.View;
using Cloudland.SSSB.Microservices.AppManagement.Extensions;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.OrganizationalPosition.Command;

public sealed class AddOrganizationalPositionCommandHandler(
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
        IRequestHandler<AddOrganizationalPositionCommand, ResultDTO<OrganizationalPositionDTO>?>
{
    public async Task<ResultDTO<OrganizationalPositionDTO>?> Handle(AddOrganizationalPositionCommand request, CancellationToken cancellationToken)
    {

            if (_httpContextAccessor.HttpContext == null)
            {
                throw new Exception("HTTP context is not available");
            }

            // Check for duplicate title
            var queryDuplicate = _repository
                .SingleResultQuery()
                .AndFilter(item => item.title == request.OrganizationalPosition.title);
            var duplicate = await _repository.SingleOrDefaultAsync(queryDuplicate, cancellationToken);
            if (duplicate != null)
            {
                return ResultDTO<OrganizationalPositionDTO>.Failure(
                    "سمت سازمانی با این عنوان از قبل موجود می باشد",
                    _mapper.Map<OrganizationalPositionDTO>(duplicate)
                );
            }

            // Map DTO to entity
            var newOrganizationalPosition = _mapper.Map<Models.OrganizationalPosition>(request.OrganizationalPosition);

            newOrganizationalPosition.CreateAt = DateTime.Now;
            newOrganizationalPosition.LastModifiedAt = DateTime.Now;
        

            if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
            {
                var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
                if (Guid.TryParse(userClaim, out Guid userId))
                {
                    newOrganizationalPosition.UserCreated = userId;
                    newOrganizationalPosition.LastModifiedUser = userId;
                }
                else
                {
                    throw new Exception("Invalid user ID in claims");
                }
            }
            else
            {
                throw new Exception("User ID claim not found");
            }

            newOrganizationalPosition.CustomerId = Guid.NewGuid();
            // Set default values
            newOrganizationalPosition.AllowDelete = true;
            newOrganizationalPosition.AllowUpdate = true;
            newOrganizationalPosition.Disabled = false;
            newOrganizationalPosition.hidden = false;

            // Add organizational chart to repository
            var organizationalPosition = await _repository.AddAsync(newOrganizationalPosition, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

            // Sync to Elasticsearch from view using repository
            try
            {
                var viewQuery = organizationalPositionViewRepository.SingleResultQuery().AndFilter(v => v.Id == newOrganizationalPosition.Id);
                var viewRow = await organizationalPositionViewRepository.SingleOrDefaultAsync(viewQuery, cancellationToken);
                if (viewRow != null)
                {
                    await _elasticClient.IndexAsync(viewRow, i => i
                        .Index(ElasticsearchIndexNames.OrganizationalPosition)
                        .Id(viewRow.Id)
                    , cancellationToken);
                }
            }
            catch
            {
                // ignore ES sync errors for now; logging can be added later
            }

            return ResultDTO<OrganizationalPositionDTO>.Success(
                _mapper.Map<OrganizationalPositionDTO>(organizationalPosition),
                "سمت سازمانی با موفقیت ایجاد شد"
            );
    }
}
