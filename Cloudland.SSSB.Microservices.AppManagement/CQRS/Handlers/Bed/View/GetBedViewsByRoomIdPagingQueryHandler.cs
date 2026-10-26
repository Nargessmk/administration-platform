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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Bed.View;

public sealed class GetBedViewsByRoomIdPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Bed> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<BedView> bedViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Bed, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetBedViewsByRoomIdPagingQuery, ResultDTO<PaginatedResult<BedViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<BedViewDTO>>> Handle(
        GetBedViewsByRoomIdPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = bedViewRepository.MultipleResultQuery()
                .AndFilter(item => item.RoomId == request.RoomId);

            var entities = await bedViewRepository.SearchAsync(query, cancellationToken);

            var paginatedResult = new PaginatedResult<BedViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(entities.Select(x => _mapper.Map<BedViewDTO>(x)));

            return ResultDTO<PaginatedResult<BedViewDTO>>.Success(
                paginatedResult,
                "لیست صفحه‌بندی شده تخت‌های اتاق مورد نظر با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<BedViewDTO>>.Failure(
                $"خطا در دریافت لیست صفحه‌بندی شده تخت‌های اتاق: {ex.Message}",
                []
            );
        }
    }
}
