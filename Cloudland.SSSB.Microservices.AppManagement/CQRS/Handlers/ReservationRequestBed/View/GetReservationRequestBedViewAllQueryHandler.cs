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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestBed.View;

public sealed class GetReservationRequestBedViewAllQueryHandler(
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
        IRequestHandler<GetReservationRequestBedViewAllQuery, ResultDTO<List<ReservationRequestBedViewDTO>>>
{
    public async Task<ResultDTO<List<ReservationRequestBedViewDTO>>> Handle(
        GetReservationRequestBedViewAllQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = ReservationRequestBedViewRepository.MultipleResultQuery();
            var entities = await ReservationRequestBedViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<ReservationRequestBedViewDTO>>(entities);

            return ResultDTO<List<ReservationRequestBedViewDTO>>.Success(
                dtos,
                "لیست کامل رزرو تخت با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<ReservationRequestBedViewDTO>>.Failure(
                $"خطا در دریافت لیست رزرو تخت: {ex.Message}",
                new List<ReservationRequestBedViewDTO>()
            );
        }
    }
}





