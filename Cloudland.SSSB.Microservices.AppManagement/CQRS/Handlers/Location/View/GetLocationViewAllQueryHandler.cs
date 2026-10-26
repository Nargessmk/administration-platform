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

public sealed class GetLocationViewAllQueryHandler(
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
        IRequestHandler<GetLocationViewAllQuery, ResultDTO<List<LocationViewDTO>>>
{
    public async Task<ResultDTO<List<LocationViewDTO>>> Handle(
        GetLocationViewAllQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = locationViewRepository.MultipleResultQuery();
            var entities = await locationViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<LocationViewDTO>>(entities);

            return ResultDTO<List<LocationViewDTO>>.Success(
                dtos,
                "لیست کامل مکان‌ها با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<LocationViewDTO>>.Failure(
                $"خطا در دریافت لیست مکان‌ها: {ex.Message}",
                new List<LocationViewDTO>()
            );
        }
    }
}
