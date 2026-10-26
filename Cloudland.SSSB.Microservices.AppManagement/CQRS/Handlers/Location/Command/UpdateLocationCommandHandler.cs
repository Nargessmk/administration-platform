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

public sealed class UpdateLocationCommandHandler(
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
        IRequestHandler<UpdateLocationCommand, ResultDTO<LocationDTO>?>
{


    public async Task<ResultDTO<LocationDTO>?> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Location.Id);
        var existingLocation = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        
        if (existingLocation == null)
        {
            return ResultDTO<LocationDTO>.Failure(
                "محل پیدا نشد",
                []
            ); 
        }
        
        if (!existingLocation.AllowUpdate)
        {
            return ResultDTO<LocationDTO>.Failure(
                "محل قابل بروزرسانی نمی باشد",
                request.Location
            ); 
        }

        // Check for duplicate title 
        if (!string.IsNullOrEmpty(request.Location.title) && request.Location.title != existingLocation.title)
        {
            var duplicateQuery = _repository
                .SingleResultQuery()
                .AndFilter(item => item.title == request.Location.title && item.Id != request.Location.Id);
            var duplicate = await _repository.SingleOrDefaultAsync(duplicateQuery, cancellationToken);
            if (duplicate != null) 
            {
                return ResultDTO<LocationDTO>.Failure(
                    "محل با این عنوان وجود دارد", 
                    _mapper.Map<LocationDTO>(duplicate) // Optionally return the duplicate
                );
            }
        }

        existingLocation.LastModifiedAt = DateTime.Now;
        
        // Get user ID from claims
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                existingLocation.LastModifiedUser = userId;
            }
        }

        existingLocation.title = request.Location.title;
        existingLocation.desc = request.Location.desc;
        existingLocation.IsActive = request.Location.IsActive;
        existingLocation.PhoneNumber = request.Location.PhoneNumber;
        existingLocation.Address = request.Location.Address;
        existingLocation.IsReservationActive = request.Location.IsReservationActive;
        existingLocation.RoomCount = request.Location.RoomCount;
        existingLocation.LocationTypeId = request.Location.LocationTypeId;
        
        
        
        var updatedLocation = _repository.Update(existingLocation);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
        //update related constant
        var locationConstantQuery = constantRepository.SingleResultQuery().AndFilter(item => item.Id == request.Location.Id);
        var locationConstant = await constantRepository.SingleOrDefaultAsync(locationConstantQuery, cancellationToken);

        locationConstant.title = existingLocation.title;
        locationConstant.LastModifiedAt = DateTime.Now;
        
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                locationConstant.LastModifiedUser = userId;
            }
        }
        constantRepository.Update(locationConstant);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch from view using repository
        try
        {
            var viewQuery = locationViewRepository.SingleResultQuery().AndFilter(v => v.Id == existingLocation.Id);
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
            _mapper.Map<LocationDTO>(updatedLocation),
            $"محل {updatedLocation.title} با موفقیت بروزرسانی شد"
        );

    }
}
