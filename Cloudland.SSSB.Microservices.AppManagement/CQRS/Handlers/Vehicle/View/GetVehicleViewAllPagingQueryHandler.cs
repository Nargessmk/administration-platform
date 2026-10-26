using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Vehicle.View;

public sealed class GetVehicleViewAllPagingQueryHandler(
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
        IRequestHandler<GetVehicleViewAllPagingQuery, ResultDTO<PaginatedResult<VehicleViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<VehicleViewDTO>>> Handle(
        GetVehicleViewAllPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = vehicleViewRepository.MultipleResultQuery();

            var entities = await vehicleViewRepository.SearchAsync(query, cancellationToken);

            var paginatedResult = new PaginatedResult<VehicleViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(entities.Select(x => _mapper.Map<VehicleViewDTO>(x)));

            return ResultDTO<PaginatedResult<VehicleViewDTO>>.Success(
                paginatedResult,
                "لیست خودروها با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<VehicleViewDTO>>.Failure(
                $"خطا در دریافت لیست خودروها: {ex.Message}",
                []
            );
        }
    }
}
