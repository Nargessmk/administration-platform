using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;
using Cloudland.SSSB.Models.View;
using Cloudland.SSSB.Microservices.AppManagement.Extensions;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Vehicle.Command;

public sealed class AddVehicleCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Vehicle> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<VehicleView> vehicleViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Vehicle, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddVehicleCommand, ResultDTO<VehicleDTO>>
{
    public async Task<ResultDTO<VehicleDTO>> Handle(AddVehicleCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }

        // Map DTO to entity
        var newVehicle = _mapper.Map<Models.Vehicle>(request.Vehicle);
        
        newVehicle.CreateAt = DateTime.Now;
        newVehicle.LastModifiedAt = DateTime.Now;
        
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                newVehicle.UserCreated = userId;
                newVehicle.LastModifiedUser = userId;
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
        newVehicle.CustomerId = Guid.NewGuid();
        // Set default values
        newVehicle.AllowDelete = true;
        newVehicle.AllowUpdate = true;
        newVehicle.Disabled = false;
        newVehicle.hidden = false;

        // Add vehicle to repository
        var vehicle = await _repository.AddAsync(newVehicle, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch from view using repository
        try
        {
            var viewQuery = vehicleViewRepository.SingleResultQuery().AndFilter(v => v.Id == newVehicle.Id);
            var viewRow = await vehicleViewRepository.SingleOrDefaultAsync(viewQuery, cancellationToken);
            if (viewRow != null)
            {
                await _elasticClient.IndexAsync(viewRow, i => i
                    .Index(ElasticsearchIndexNames.Vehicle)
                    .Id(viewRow.Id)
                , cancellationToken);
            }
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }
        
        return ResultDTO<VehicleDTO>.Success(
            _mapper.Map<VehicleDTO>(vehicle),
            "وسیله نقلیه با موفقیت ایجاد شد"
        );
    }
}
