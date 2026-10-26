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

public sealed class GetReservationRequestHeaderViewAllQueryHandler(
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
        IRequestHandler<GetReservationRequestHeaderViewAllQuery, ResultDTO<List<ReservationRequestHeaderViewDTO>>>
{
    public async Task<ResultDTO<List<ReservationRequestHeaderViewDTO>>> Handle(
        GetReservationRequestHeaderViewAllQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = ReservationRequestHeaderViewRepository.MultipleResultQuery()
                .OrderByDescending(d => d.CreateAt);
            var entities = await ReservationRequestHeaderViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<ReservationRequestHeaderViewDTO>>(entities);

            return ResultDTO<List<ReservationRequestHeaderViewDTO>>.Success(
                dtos,
                "لیست کامل درخواست‌های رزرو با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<ReservationRequestHeaderViewDTO>>.Failure(
                $"خطا در دریافت لیست درخواست‌های رزرو: {ex.Message}",
                new List<ReservationRequestHeaderViewDTO>()
            );
        }
    }
}


