using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.VehicleDriver.Query;

public sealed class GetVehicleDriverDriverByIdQueryHandler(
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
        IRequestHandler<GetVehicleDriverByIdQuery, ResultDTO<VehicleDriverDTO>>
{
    public async Task<ResultDTO<VehicleDriverDTO>> Handle(GetVehicleDriverByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext 
                ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            var vehicleDriver = await _dbContext.VehicleDrivers
                .Where(x => x.Id == request.Id)
                .Where(x => !x.Disabled)
                .Where(x => !x.hidden)   
                .FirstOrDefaultAsync(cancellationToken);

            if (vehicleDriver == null)
            {
                return ResultDTO<VehicleDriverDTO>.Failure(
                    " راننده وسیله نقلیه مورد نظر یافت نشد ",
                    []
                );
            }

            return ResultDTO<VehicleDriverDTO>.Success(
                _mapper.Map<VehicleDriverDTO>(vehicleDriver),
                "راننده وسیله نقلیه با موفقیت برگشت داده شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<VehicleDriverDTO>.Failure(
                $"خطا در دریافت راننده وسیله نقلیه: {ex.Message}",
                []
            );
        }
    }
}
