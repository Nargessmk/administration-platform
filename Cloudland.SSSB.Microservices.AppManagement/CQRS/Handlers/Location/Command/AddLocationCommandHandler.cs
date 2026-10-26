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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Location.Command;

public sealed class AddLocationCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Location> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<LocationView> locationViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Location, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddLocationCommand, ResultDTO<LocationDTO>?>
{
    public async Task<ResultDTO<LocationDTO>?> Handle(AddLocationCommand request, CancellationToken cancellationToken)
    {

            if (_httpContextAccessor.HttpContext == null)
            {
                throw new Exception("HTTP context is not available");
            }

            // Check for duplicate title
            var queryDuplicate = _repository
                .SingleResultQuery()
                .AndFilter(item => item.title == request.Location.title);
            var duplicate = await _repository.SingleOrDefaultAsync(queryDuplicate, cancellationToken);
            if (duplicate != null)
            {
                return ResultDTO<LocationDTO>.Failure(
                    "محلی با این عنوان از قبل موجود می باشد",
                    _mapper.Map<LocationDTO>(duplicate)
                );
            }

            // Map DTO to entity
            var newLocation = _mapper.Map<Models.Location>(request.Location);

            newLocation.CreateAt = DateTime.Now;
            newLocation.LastModifiedAt = DateTime.Now;
        

            if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
            {
                var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
                if (Guid.TryParse(userClaim, out Guid userId))
                {
                    newLocation.UserCreated = userId;
                    newLocation.LastModifiedUser = userId;
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

            newLocation.CustomerId = Guid.NewGuid();
            // Set default values
            newLocation.AllowDelete = true;
            newLocation.AllowUpdate = true;
            newLocation.Disabled = false;
            newLocation.hidden = false;

            
            var location = await _repository.AddAsync(newLocation, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

            // Sync to Elasticsearch from view using repository
            try
            {
                var viewQuery = locationViewRepository.SingleResultQuery().AndFilter(v => v.Id == newLocation.Id);
                var viewRow = await locationViewRepository.SingleOrDefaultAsync(viewQuery, cancellationToken);
                if (viewRow != null)
                {
                    await _elasticClient.IndexAsync(viewRow, i => i
                        .Index(ElasticsearchIndexNames.Location)
                        .Id(viewRow.Id)
                    , cancellationToken);
                }
            }
            catch
            {
                // ignore ES sync errors for now; logging can be added later
            }

            return ResultDTO<LocationDTO>.Success(
                _mapper.Map<LocationDTO>(location),
                "محل با موفقیت ایجاد شد"
            );
    }
}
