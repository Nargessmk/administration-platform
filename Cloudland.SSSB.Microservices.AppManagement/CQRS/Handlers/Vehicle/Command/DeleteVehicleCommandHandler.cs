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
using Cloudland.SSSB.Microservices.AppManagement.Extensions;
using Cloudland.SSSB.Models.View;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Vehicle.Command;

public sealed class DeleteVehicleCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Vehicle> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Vehicle, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteVehicleCommand, ResultDTO<int>>
{
        public async Task<ResultDTO<int>> Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }

        // Find the vehicle to delete
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
        var vehicleToDelete = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        
        if (vehicleToDelete == null)
        {
            return ResultDTO<int>.Failure(
                $"وسیله نقلیه‌ای با این شناسه برای حذف پیدا نشد",
                0
            ); // No rows affected
        }
        
        if (!vehicleToDelete.AllowDelete)
        {
            return ResultDTO<int>.Failure(
                "وسیله نقلیه قابل حذف نمی باشد",
                0
            ); // No rows affected
        }
        
        if (vehicleToDelete.Disabled)
        {
            // Already disabled, just return success
            return ResultDTO<int>.Success(0, "وسیله نقلیه قبلاً غیرفعال شده است");
        }

        vehicleToDelete.Disabled = true;
        vehicleToDelete.LastModifiedAt = DateTime.UtcNow;
        
        // Get user ID from claims
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                vehicleToDelete.LastModifiedUser = userId;
            }
        }
        
        _repository.Update(vehicleToDelete);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        try
        {
            await _elasticClient.DeleteAsync<VehicleView>(request.Id, d => d
                .Index(ElasticsearchIndexNames.Vehicle)
            , cancellationToken);
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }

        return ResultDTO<int>.Success(
            1,
            $"وسیله نقلیه با موفقیت غیرفعال شد"
        );
    }
}
