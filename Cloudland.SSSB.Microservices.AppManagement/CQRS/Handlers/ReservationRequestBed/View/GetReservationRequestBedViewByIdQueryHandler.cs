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

public sealed class GetReservationRequestBedViewByIdQueryHandler(
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
        IRequestHandler<GetReservationRequestBedViewByIdQuery, ResultDTO<ReservationRequestBedViewDTO>>
{
    public async Task<ResultDTO<ReservationRequestBedViewDTO>> Handle(
        GetReservationRequestBedViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = ReservationRequestBedViewRepository.SingleResultQuery()
                .AndFilter(x => x.Id == request.Id);
            var entity = await ReservationRequestBedViewRepository.SingleOrDefaultAsync(query, cancellationToken);

            if (entity == null)
            {
                return ResultDTO<ReservationRequestBedViewDTO>.Failure(
                    "رزرو تخت یافت نشد",
                    []
                );
            }

            var dto = _mapper.Map<ReservationRequestBedViewDTO>(entity);

            return ResultDTO<ReservationRequestBedViewDTO>.Success(
                dto,
                "رزرو تخت با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<ReservationRequestBedViewDTO>.Failure(
                $"خطا در دریافت رزرو تخت: {ex.Message}",
                []
            );
        }
    }
}





