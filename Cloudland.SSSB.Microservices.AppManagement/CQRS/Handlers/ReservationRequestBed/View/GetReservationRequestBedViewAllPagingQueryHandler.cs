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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestBed.View;

public sealed class GetReservationRequestBedViewAllPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestBed> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ReservationRequestBedView> ReservationRequestBedViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ReservationRequestBed, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
        distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetReservationRequestBedViewAllPagingQuery, ResultDTO<PaginatedResult<ReservationRequestBedViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<ReservationRequestBedViewDTO>>> Handle(
        GetReservationRequestBedViewAllPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = ReservationRequestBedViewRepository.MultipleResultQuery();
            var entities = await ReservationRequestBedViewRepository.SearchAsync(query, cancellationToken);
            var mappedEntities = entities.Select(x => _mapper.Map<ReservationRequestBedViewDTO>(x));

            var paginatedResult = new PaginatedResult<ReservationRequestBedViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(mappedEntities);

            return ResultDTO<PaginatedResult<ReservationRequestBedViewDTO>>.Success(
                paginatedResult,
                "لیست رزرو تخت‌ها با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ReservationRequestBedViewDTO>>.Failure(
                $"خطا در دریافت لیست رزرو تخت‌ها: {ex.Message}",
                []
            );
        }
    }
}





