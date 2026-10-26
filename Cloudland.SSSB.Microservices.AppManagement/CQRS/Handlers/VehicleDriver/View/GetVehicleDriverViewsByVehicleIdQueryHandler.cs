using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.VehicleDriver.View;

public sealed class GetVehicleDriverViewsByVehicleIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.VehicleDriver> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<VehicleDriverView> vehicleDriverViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.VehicleDriver, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetVehicleDriverViewsByVehicleIdQuery, ResultDTO<List<VehicleDriverViewDTO>>>
{
    public async Task<ResultDTO<List<VehicleDriverViewDTO>>> Handle(
        GetVehicleDriverViewsByVehicleIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = vehicleDriverViewRepository.MultipleResultQuery()
                .AndFilter(item => item.VehicleId == request.VehicleId);

            var entities = await vehicleDriverViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<VehicleDriverViewDTO>>(entities);

            return ResultDTO<List<VehicleDriverViewDTO>>.Success(
                dtos,
                "لیست رانندگان خودرو مورد نظر با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<VehicleDriverViewDTO>>.Failure(
                $"خطا در دریافت لیست رانندگان خودرو: {ex.Message}",
                new List<VehicleDriverViewDTO>()
            );
        }
    }
}
