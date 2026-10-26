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

public sealed class GetVehicleViewChildrenByParentIdPagingQueryHandler(
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
        IRequestHandler<GetVehicleViewChildrenByParentIdPagingQuery, ResultDTO<PaginatedResult<VehicleViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<VehicleViewDTO>>> Handle(
        GetVehicleViewChildrenByParentIdPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = vehicleViewRepository
                .MultipleResultQuery()
                .AndFilter(x => x.ParentId == request.ParentId)
                .OrderByDescending(x => x.CreateAt);

            var entities = await vehicleViewRepository.SearchAsync(query, cancellationToken);
            var mappedEntities = entities.Select(x => _mapper.Map<VehicleViewDTO>(x));

            var paginatedResult = new PaginatedResult<VehicleViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(mappedEntities);

            return ResultDTO<PaginatedResult<VehicleViewDTO>>.Success(
                paginatedResult,
                "لیست فرزندان خودرو با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<VehicleViewDTO>>.Failure(
                $"خطا در دریافت لیست فرزندان خودرو: {ex.Message}",
                []
            );
        }
    }
}
