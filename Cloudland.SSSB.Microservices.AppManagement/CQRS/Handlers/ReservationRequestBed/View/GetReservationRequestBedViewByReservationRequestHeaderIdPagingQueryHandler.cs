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

public sealed class GetReservationRequestBedViewByReservationRequestHeaderIdPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestBed> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ReservationRequestBedView> reservationRequestBedViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ReservationRequestBed, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetReservationRequestBedViewByReservationRequestHeaderIdPagingQuery, ResultDTO<PaginatedResult<ReservationRequestBedViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<ReservationRequestBedViewDTO>>> Handle(
        GetReservationRequestBedViewByReservationRequestHeaderIdPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = reservationRequestBedViewRepository.MultipleResultQuery()
                .AndFilter(item => item.ReservationRequestHeaderId == request.ReservationRequestId)
                .OrderByDescending(d => d.CreateAt);

            var entities = await reservationRequestBedViewRepository.SearchAsync(query, cancellationToken);

            var paginatedResult = new PaginatedResult<ReservationRequestBedViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(entities.Select(x => _mapper.Map<ReservationRequestBedViewDTO>(x)));

            return ResultDTO<PaginatedResult<ReservationRequestBedViewDTO>>.Success(
                paginatedResult,
                "لیست صفحه‌بندی شده رزرو تخت بر اساس شناسه هدر رزرو با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ReservationRequestBedViewDTO>>.Failure(
                $"خطا در دریافت لیست صفحه‌بندی شده رزرو تخت بر اساس شناسه هدر رزرو: {ex.Message}",
                []
            );
        }
    }
}
