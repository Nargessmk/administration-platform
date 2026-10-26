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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Location.View;

public sealed class GetLocationViewByIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Location> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<LocationView> locationViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Location, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetLocationViewByIdQuery, ResultDTO<LocationViewDTO>>
{
    public async Task<ResultDTO<LocationViewDTO>> Handle(
        GetLocationViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = locationViewRepository
                .SingleResultQuery()
                .AndFilter(x => x.Id == request.Id && !x.Disabled && !x.hidden);

            var entity = await locationViewRepository.SingleOrDefaultAsync(query, cancellationToken);

            if (entity is null)
            {
                return ResultDTO<LocationViewDTO>.Failure(
                    "مکان مورد نظر یافت نشد",
                    []
                );
            }

            var dto = _mapper.Map<LocationViewDTO>(entity);

            return ResultDTO<LocationViewDTO>.Success(
                dto,
                "مکان با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<LocationViewDTO>.Failure(
                $"خطا در دریافت اطلاعات مکان: {ex.Message}",
                []
            );
        }
    }
}
