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

public sealed class UpdateVehicleCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Vehicle> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<APM.Models.Constant> constantRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<VehicleView> vehicleViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Vehicle, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<UpdateVehicleCommand, ResultDTO<VehicleDTO>>
{
    public async Task<ResultDTO<VehicleDTO>> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Vehicle.Id);
        var existingVehicle = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        
        if (existingVehicle == null)
        {
            return ResultDTO<VehicleDTO>.Failure(
                "وسیله نقلیه پیدا نشد",
                request.Vehicle
            ); 
        }
        
        if (!existingVehicle.AllowUpdate)
        {
            return ResultDTO<VehicleDTO>.Failure(
                "وسیله نقلیه قابل بروزرسانی نمی باشد",
                request.Vehicle
            ); 
        }

        existingVehicle.LastModifiedAt = DateTime.Now;
        
        // Get user ID from claims
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                existingVehicle.LastModifiedUser = userId;
            }
        }
        
       existingVehicle.Capacity = request.Vehicle.Capacity;
       existingVehicle.IsActive = request.Vehicle.IsActive;
       existingVehicle.LicensePlate = request.Vehicle.LicensePlate;
       existingVehicle.ValidityDate = _mapper.Map<DateTime?>(request.Vehicle.ValidityDate);
       existingVehicle.VehicleTypeId = request.Vehicle.VehicleTypeId;
       existingVehicle.desc = request.Vehicle.desc;
       existingVehicle.title = request.Vehicle.title;
       
        
        
        var updatedVehicle = _repository.Update(existingVehicle);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
        
        //update related constant
        var vehicleConstantQuery = constantRepository.SingleResultQuery().AndFilter(item => item.Id == request.Vehicle.Id);
        var vehicleConstant = await constantRepository.SingleOrDefaultAsync(vehicleConstantQuery, cancellationToken);

        vehicleConstant.title = $"{request.Vehicle.LicensePlate +'-'+request.Vehicle.TypeTitle}";
        vehicleConstant.LastModifiedAt = DateTime.Now;
        
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                vehicleConstant.LastModifiedUser = userId;
            }
        }
        constantRepository.Update(vehicleConstant);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch from view using repository
        try
        {
            var viewQuery = vehicleViewRepository.SingleResultQuery().AndFilter(v => v.Id == existingVehicle.Id);
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
            _mapper.Map<VehicleDTO>(updatedVehicle),
            $"وسیله نقلیه با موفقیت بروزرسانی شد"
        );
    }
}
