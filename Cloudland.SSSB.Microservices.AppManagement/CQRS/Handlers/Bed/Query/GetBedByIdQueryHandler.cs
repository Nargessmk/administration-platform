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

public sealed class GetBedByIdQueryHandler(
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
        IRequestHandler<GetBedByIdQuery, ResultDTO<BedDTO>>
{
    public async Task<ResultDTO<BedDTO>> Handle(GetBedByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _repository.SingleResultQuery()
                .AndFilter(item => item.Id == request.Id)
                .AndFilter(item => !item.Disabled)
                .AndFilter(item => !item.hidden);
            
            var bed = await _repository.SingleOrDefaultAsync(query, cancellationToken);

            if (bed == null)
            {
                return ResultDTO<BedDTO>.Failure(
                    "تخت مورد نظر یافت نشد ",
                    []
                );
            }

            return ResultDTO<BedDTO>.Success(
                _mapper.Map<BedDTO>(bed),
                "تخت با موفقیت برگشت داده شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<BedDTO>.Failure(
                $"خطا در دریافت تخت: {ex.Message}",
                []
            );
        }
    }
}
