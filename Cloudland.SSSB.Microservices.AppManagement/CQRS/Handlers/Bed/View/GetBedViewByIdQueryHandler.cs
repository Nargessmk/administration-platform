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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Bed.View;

public sealed class GetBedViewByIdQueryHandler(
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
        IRequestHandler<GetBedViewByIdQuery, ResultDTO<BedViewDTO>>
{
    public async Task<ResultDTO<BedViewDTO>> Handle(
        GetBedViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = bedViewRepository.SingleResultQuery()
                .AndFilter(item => item.Id == request.Id);

            var entity = await bedViewRepository.SingleOrDefaultAsync(query, cancellationToken);

            if (entity == null)
            {
                return ResultDTO<BedViewDTO>.Failure(
                    "تخت مورد نظر یافت نشد",
                    []
                );
            }

            var dto = _mapper.Map<BedViewDTO>(entity);

            return ResultDTO<BedViewDTO>.Success(
                dto,
                "تخت با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<BedViewDTO>.Failure(
                $"خطا در دریافت تخت: {ex.Message}",
                []
            );
        }
    }
}
