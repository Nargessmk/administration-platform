using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.Extensions;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Location.Command;

public sealed class AddLocationWithParentCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Location> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<APM.Models.Constant> constantRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<LocationView> locationViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Location, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddLocationWithConstantCommand, ResultDTO<LocationDTO>?>
{
    public async Task<ResultDTO<LocationDTO>?> Handle(AddLocationWithConstantCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }

        var duplicateQuery = _repository
            .SingleResultQuery()
            .AndFilter(item => item.title == request.Location.title);
        var duplicateLocation = await _repository.SingleOrDefaultAsync(duplicateQuery, cancellationToken);
        if (duplicateLocation != null)
        {
            return ResultDTO<LocationDTO>.Failure(
                "محلی با این عنوان از قبل موجود می باشد",
                _mapper.Map<LocationDTO>(duplicateLocation)
            );
        }

        var newConstant = new APM.Models.Constant
        {
            Id = Guid.NewGuid(),
            ParentId = request.ParentId,
            CreateAt = DateTime.Now,
            LastModifiedAt = DateTime.Now
        };
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid constantUserId))
            {
                newConstant.UserCreated = constantUserId;
                newConstant.LastModifiedUser = constantUserId;
            }
        }
        newConstant.CustomerId = Guid.NewGuid();
        newConstant.AllowDelete = true;
        newConstant.AllowUpdate = true;
        newConstant.Disabled = false;
        newConstant.hidden = false;
        newConstant.title = request.Location.title;
        newConstant.desc = request.Location.desc;
        newConstant.Icon = "error_outline";
        newConstant.NodeTypeId = Guid.Parse("f0042ac1-d377-47ea-ae76-85b3f5616fc9");
        newConstant.ConstantValue = "Location";

        var addedConstant = await constantRepository.AddAsync(newConstant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
        var locationEntity = _mapper.Map<Models.Location>(request.Location);

        locationEntity.Id = addedConstant.Id;
        locationEntity.CreateAt = DateTime.Now;
        locationEntity.LastModifiedAt = DateTime.Now;

        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                locationEntity.UserCreated = userId;
                locationEntity.LastModifiedUser = userId;
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

        locationEntity.CustomerId = Guid.NewGuid();
        locationEntity.AllowDelete = true;
        locationEntity.AllowUpdate = true;
        locationEntity.Disabled = false;
        locationEntity.hidden = false;

        var added = await _repository.AddAsync(locationEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
        
        // Sync to Elasticsearch from view using repository
        try
        {
            var viewQuery = locationViewRepository.SingleResultQuery().AndFilter(v => v.Id == locationEntity.Id);
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
            _mapper.Map<LocationDTO>(added),
            "محل با  موفقیت ایجاد شد"
        );
    }
}


