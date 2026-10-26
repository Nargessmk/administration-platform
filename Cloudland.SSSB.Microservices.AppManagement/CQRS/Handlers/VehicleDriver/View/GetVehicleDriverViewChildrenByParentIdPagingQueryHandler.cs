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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.VehicleDriver.View;

public sealed class GetVehicleDriverViewChildrenByParentIdPagingQueryHandler(
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
        IRequestHandler<GetVehicleDriverViewChildrenByParentIdPagingQuery, ResultDTO<PaginatedResult<VehicleDriverViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<VehicleDriverViewDTO>>> Handle(
        GetVehicleDriverViewChildrenByParentIdPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = vehicleDriverViewRepository.MultipleResultQuery()
                .AndFilter(x => x.ParentId == request.ParentId);

            var entities = await vehicleDriverViewRepository.SearchAsync(query, cancellationToken);

            var paginatedResult = new PaginatedResult<VehicleDriverViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(entities.Select(x => _mapper.Map<VehicleDriverViewDTO>(x)));

            return ResultDTO<PaginatedResult<VehicleDriverViewDTO>>.Success(
                paginatedResult,
                "لیست رانندگان خودرو فرزند با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<VehicleDriverViewDTO>>.Failure(
                $"خطا در دریافت لیست رانندگان خودرو فرزند: {ex.Message}",
                []
            );
        }
    }
}
