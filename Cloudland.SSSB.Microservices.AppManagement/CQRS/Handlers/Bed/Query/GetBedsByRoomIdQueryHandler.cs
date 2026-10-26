using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Bed.Query;

public sealed class GetBedsByRoomIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Bed> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Bed, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetBedsByRoomIdQuery, ResultDTO<List<BedDTO>>>
{
    public async Task<ResultDTO<List<BedDTO>>> Handle(GetBedsByRoomIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _repository
                .MultipleResultQuery()
                .AndFilter(x => x.RoomId == request.RoomId)
                .AndFilter(x => !x.Disabled)
                .AndFilter(x => !x.hidden)
                .OrderBy(x => x.title);

            var beds = await _repository.SearchAsync(query, cancellationToken);
            var bedDtos = beds.Select(x => _mapper.Map<BedDTO>(x)).ToList();

            return ResultDTO<List<BedDTO>>.Success(
                bedDtos,
                $"لیست تخت‌های اتاق با شناسه {request.RoomId} با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<BedDTO>>.Failure(
                $"خطا در دریافت لیست تخت‌های اتاق: {ex.Message}",
                [""]
            );
        }
    }
}
