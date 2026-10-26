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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestHeader.View;

public sealed class GetReservationRequestHeaderViewByIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestHeader> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ReservationRequestHeaderView> ReservationRequestHeaderViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ReservationRequestHeader, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetReservationRequestHeaderViewByIdQuery, ResultDTO<ReservationRequestHeaderViewDTO>>
{
    public async Task<ResultDTO<ReservationRequestHeaderViewDTO>> Handle(
        GetReservationRequestHeaderViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = ReservationRequestHeaderViewRepository.SingleResultQuery()
                .AndFilter(x => x.Id == request.Id);
            var entity = await ReservationRequestHeaderViewRepository.SingleOrDefaultAsync(query, cancellationToken);

            if (entity == null)
            {
                return ResultDTO<ReservationRequestHeaderViewDTO>.Failure(
                    "درخواست رزرو یافت نشد",
                    []
                );
            }

            var dto = _mapper.Map<ReservationRequestHeaderViewDTO>(entity);

            return ResultDTO<ReservationRequestHeaderViewDTO>.Success(
                dto,
                "درخواست رزرو با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<ReservationRequestHeaderViewDTO>.Failure(
                $"خطا در دریافت درخواست رزرو: {ex.Message}",
                []
            );
        }
    }
}


