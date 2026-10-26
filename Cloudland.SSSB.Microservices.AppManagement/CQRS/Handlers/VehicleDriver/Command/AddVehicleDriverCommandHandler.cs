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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.VehicleDriver.Command;

public sealed class AddVehicleDriverDriverCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.VehicleDriver> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.VehicleDriver, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddVehicleDriverCommand, ResultDTO<VehicleDriverDTO>>
{
    public async Task<ResultDTO<VehicleDriverDTO>> Handle(AddVehicleDriverCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        // Map DTO to entity
        var newVehicleDriver = _mapper.Map<Models.VehicleDriver>(request.VehicleDriver);
        
        newVehicleDriver.CreateAt = DateTime.Now;
        newVehicleDriver.LastModifiedAt = DateTime.Now;
        
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                newVehicleDriver.UserCreated = userId;
                newVehicleDriver.LastModifiedUser = userId;
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
        newVehicleDriver.CustomerId = Guid.NewGuid();
        // Set default values
        newVehicleDriver.AllowDelete = true;
        newVehicleDriver.AllowUpdate = true;
        newVehicleDriver.Disabled = false;
        newVehicleDriver.hidden = false;

        // Add VehicleDriver to repository
        var vehicleDriver = await _repository.AddAsync(newVehicleDriver, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
        return ResultDTO<VehicleDriverDTO>.Success(
            _mapper.Map<VehicleDriverDTO>(vehicleDriver),
            " راننده وسیله نقلیه با موفقیت ایجاد شد"
        );
    }
}
