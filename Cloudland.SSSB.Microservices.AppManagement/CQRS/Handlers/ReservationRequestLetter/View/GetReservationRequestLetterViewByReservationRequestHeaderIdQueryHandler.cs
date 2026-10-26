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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestLetter.View;

public sealed class GetReservationRequestLetterViewByReservationRequestHeaderIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ReservationRequestLetter> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ReservationRequestLetterView> letterViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ReservationRequestLetter, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetReservationRequestLetterViewByReservationRequestHeaderIdQuery, ResultDTO<ReservationRequestLetterViewDTO>>
{
    public async Task<ResultDTO<ReservationRequestLetterViewDTO>> Handle(
        GetReservationRequestLetterViewByReservationRequestHeaderIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = letterViewRepository.SingleResultQuery()
                .AndFilter(item => item.ReservationRequestHeaderId == request.ReservationRequestId);

            var entities = await letterViewRepository.SingleOrDefaultAsync(query, cancellationToken);

            var dto = _mapper.Map<ReservationRequestLetterViewDTO>(entities);

            return ResultDTO<ReservationRequestLetterViewDTO>.Success(
                dto,
                " نامه‌ بر اساس شناسه هدر رزرو با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<ReservationRequestLetterViewDTO>.Failure(
                $"خطا در دریافت  نامه بر اساس شناسه هدر رزرو: {ex.Message}",
                new ReservationRequestLetterViewDTO()
            );
        }
    }
}
