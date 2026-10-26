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

public sealed class GetVehicleDriverViewByIdQueryHandler(
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
        IRequestHandler<GetVehicleDriverViewByIdQuery, ResultDTO<VehicleDriverViewDTO>>
{
    public async Task<ResultDTO<VehicleDriverViewDTO>> Handle(
        GetVehicleDriverViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = vehicleDriverViewRepository.SingleResultQuery()
                .AndFilter(item => item.Id == request.Id);

            var entity = await vehicleDriverViewRepository.SingleOrDefaultAsync(query, cancellationToken);

            if (entity == null)
            {
                return ResultDTO<VehicleDriverViewDTO>.Failure(
                    "راننده خودرو مورد نظر یافت نشد",
                    []
                );
            }

            var dto = _mapper.Map<VehicleDriverViewDTO>(entity);

            return ResultDTO<VehicleDriverViewDTO>.Success(
                dto,
                "راننده خودرو با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<VehicleDriverViewDTO>.Failure(
                $"خطا در دریافت راننده خودرو: {ex.Message}",
                []
            );
        }
    }
}
