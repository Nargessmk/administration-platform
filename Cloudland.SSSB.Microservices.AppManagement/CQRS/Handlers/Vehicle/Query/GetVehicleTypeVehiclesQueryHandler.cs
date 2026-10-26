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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Vehicle.Query;

public sealed class GetVehicleTypeVehiclesQueryHandler(
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
        IRequestHandler<GetVehicleTypeVehiclesQuery, ResultDTO<List<VehicleDTO>>>
{
    public async Task<ResultDTO<List<VehicleDTO>>> Handle(GetVehicleTypeVehiclesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var db = _unitOfWork.DbContext as SSSBDBContext 
                ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            var vehicles = await db.Vehicles
                .Where(x => x.VehicleTypeId == request.VehicleTypeId)
                .Where(x => !x.Disabled && !x.hidden)
                .OrderBy(x => x.title)
                .ToListAsync(cancellationToken);

            var dto = vehicles.Select(x => _mapper.Map<VehicleDTO>(x)).ToList();
            return ResultDTO<List<VehicleDTO>>.Success(dto, "لیست وسایل نقلیه نوع مشخص شده با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            return ResultDTO<List<VehicleDTO>>.Failure(
                $"خطا در دریافت لیست وسایل نقلیه نوع مشخص شده: {ex.Message}",
                new List<VehicleDTO>()
            );
        }
    }
}
