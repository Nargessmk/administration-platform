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

public sealed class UpdateVehicleDriverDriverCommandHandler(
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
        IRequestHandler<UpdateVehicleDriverCommand, ResultDTO<VehicleDriverDTO>>
{
    public async Task<ResultDTO<VehicleDriverDTO>> Handle(UpdateVehicleDriverCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.VehicleDriver.Id);
        var existingVehicleDriver = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        
        if (existingVehicleDriver == null)
        {
            return ResultDTO<VehicleDriverDTO>.Failure(
                "راننده وسیله نقلیه پیدا نشد",
                request.VehicleDriver
            ); 
        }
        
        if (!existingVehicleDriver.AllowUpdate)
        {
            return ResultDTO<VehicleDriverDTO>.Failure(
                "راننده وسیله نقلیه قابل بروزرسانی نمی باشد",
                request.VehicleDriver
            ); 
        }

        existingVehicleDriver.LastModifiedAt = DateTime.Now;
        
        // Get user ID from claims
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                existingVehicleDriver.LastModifiedUser = userId;
            }
        }
        
        existingVehicleDriver.PersonnelId = request.VehicleDriver.PersonnelId;
        existingVehicleDriver.DepartmentId = request.VehicleDriver.DepartmentId;
        existingVehicleDriver.VehicleId = request.VehicleDriver.VehicleId;
        existingVehicleDriver.title = request.VehicleDriver.title;
        existingVehicleDriver.desc = request.VehicleDriver.desc;
        
        
        var updatedVehicleDriver = _repository.Update(existingVehicleDriver);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        return ResultDTO<VehicleDriverDTO>.Success(
            _mapper.Map<VehicleDriverDTO>(updatedVehicleDriver),
            $" راننده وسیله نقلیه با موفقیت بروزرسانی شد"
        );
    }
}
