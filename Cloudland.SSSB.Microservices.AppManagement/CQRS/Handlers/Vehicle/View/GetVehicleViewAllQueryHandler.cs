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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Vehicle.View;

public sealed class GetVehicleViewAllQueryHandler(
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
        IRequestHandler<GetVehicleViewAllQuery, ResultDTO<List<VehicleViewDTO>>>
{
    public async Task<ResultDTO<List<VehicleViewDTO>>> Handle(
        GetVehicleViewAllQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = vehicleViewRepository.MultipleResultQuery();

            var entities = await vehicleViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<VehicleViewDTO>>(entities);

            return ResultDTO<List<VehicleViewDTO>>.Success(
                dtos,
                "لیست کامل خودروها با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<VehicleViewDTO>>.Failure(
                $"خطا در دریافت لیست خودروها: {ex.Message}",
                new List<VehicleViewDTO>()
            );
        }
    }
}
