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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ReservationRequestLetter.View;

public sealed class GetReservationRequestLetterViewByReservationRequestHeaderIdPagingQueryHandler(
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
        IRequestHandler<GetReservationRequestLetterViewByReservationRequestHeaderIdPagingQuery, ResultDTO<PaginatedResult<ReservationRequestLetterViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<ReservationRequestLetterViewDTO>>> Handle(
        GetReservationRequestLetterViewByReservationRequestHeaderIdPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = letterViewRepository.MultipleResultQuery()
                .AndFilter(item => item.ReservationRequestHeaderId == request.ReservationRequestId);

            var entities = await letterViewRepository.SearchAsync(query, cancellationToken);

            var paginatedResult = new PaginatedResult<ReservationRequestLetterViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(entities.Select(x => _mapper.Map<ReservationRequestLetterViewDTO>(x)));

            return ResultDTO<PaginatedResult<ReservationRequestLetterViewDTO>>.Success(
                paginatedResult,
                "لیست صفحه‌بندی شده نامه‌ها بر اساس شناسه هدر رزرو با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ReservationRequestLetterViewDTO>>.Failure(
                $"خطا در دریافت لیست صفحه‌بندی شده نامه‌ها بر اساس شناسه هدر رزرو: {ex.Message}",
                []
            );
        }
    }
}
