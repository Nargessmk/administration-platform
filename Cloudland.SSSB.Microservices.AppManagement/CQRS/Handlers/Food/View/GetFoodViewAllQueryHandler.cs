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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Food.View;

public sealed class GetFoodViewAllQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Food> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<FoodView> foodViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Food, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetFoodViewAllQuery, ResultDTO<List<FoodViewDTO>>>
{
    public async Task<ResultDTO<List<FoodViewDTO>>> Handle(
        GetFoodViewAllQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = foodViewRepository.MultipleResultQuery();

            var entities = await foodViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<FoodViewDTO>>(entities);

            return ResultDTO<List<FoodViewDTO>>.Success(
                dtos,
                "لیست کامل غذاها با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<FoodViewDTO>>.Failure(
                $"خطا در دریافت لیست غذاها: {ex.Message}",
                new List<FoodViewDTO>()
            );
        }
    }
}
