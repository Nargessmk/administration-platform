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

public sealed class DeleteVehicleDriverCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.VehicleDriver> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<DeleteVehicleDriverCommandHandler> logger)
    : BaseHandler<Models.VehicleDriver, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteVehicleDriverCommand, ResultDTO<int>>
{
        public async Task<ResultDTO<int>> Handle(DeleteVehicleDriverCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }

        // Find the VehicleDriver to delete
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
        var vehicleDriverToDelete = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        
        if (vehicleDriverToDelete == null)
        {
            return ResultDTO<int>.Failure(
                $"راننده وسیله نقلیه‌ای با این شناسه برای حذف پیدا نشد",
                0
            ); // No rows affected
        }
        
        if (!vehicleDriverToDelete.AllowDelete)
        {
            return ResultDTO<int>.Failure(
                " راننده وسیله نقلیه قابل حذف نمی باشد",
                0
            ); // No rows affected
        }
        
        if (vehicleDriverToDelete.Disabled)
        {
            // Already disabled, just return success
            return ResultDTO<int>.Success(0, "راننده وسیله نقلیه قبلاً غیرفعال شده است");
        }
        if (vehicleDriverToDelete.Disabled)
        {
            logger.LogInformation($"VehicleDriver with ID {request.Id} is disabled, proceeding with deletion");
        }

        var affectedRows = await _repository.RemoveAsync(x => x.Id == request.Id, cancellationToken);
        
        if (affectedRows > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        }
        else
        {
            logger.LogWarning($"No rows were affected when deleting VehicleDriver with ID {request.Id}");
        }

        return ResultDTO<int>.Success(
            affectedRows,
            
            $"راننده {vehicleDriverToDelete.title} با موفقیت حذف شد"
        );
    }
}
