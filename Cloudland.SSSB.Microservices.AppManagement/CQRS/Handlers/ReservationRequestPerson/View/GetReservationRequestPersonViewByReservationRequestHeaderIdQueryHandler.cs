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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestPerson.View;

public sealed class GetReservationRequestPersonViewByReservationRequestHeaderIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestPerson> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ReservationRequestPersonView> reservationRequestPersonViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ReservationRequestPerson, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetReservationRequestPersonViewByReservationRequestHeaderIdQuery, ResultDTO<List<ReservationRequestPersonViewDTO>>>
{
    public async Task<ResultDTO<List<ReservationRequestPersonViewDTO>>> Handle(
        GetReservationRequestPersonViewByReservationRequestHeaderIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = reservationRequestPersonViewRepository.MultipleResultQuery()
                .AndFilter(item => item.ReservationRequestHeaderId == request.ReservationRequestId)
                .OrderByDescending(d => d.CreateAt);

            var entities = await reservationRequestPersonViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<ReservationRequestPersonViewDTO>>(entities);

            return ResultDTO<List<ReservationRequestPersonViewDTO>>.Success(
                dtos,
                "لیست پرسنل رزرو بر اساس شناسه هدر رزرو با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<ReservationRequestPersonViewDTO>>.Failure(
                $"خطا در دریافت لیست پرسنل رزرو بر اساس شناسه هدر رزرو: {ex.Message}",
                new List<ReservationRequestPersonViewDTO>()
            );
        }
    }
}
