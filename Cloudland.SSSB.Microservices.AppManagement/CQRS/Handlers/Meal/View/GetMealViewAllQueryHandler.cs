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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Meal.View;

public sealed class GetMealViewAllQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Meal> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<MealView> mealViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Meal, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetMealViewAllQuery, ResultDTO<List<MealViewDTO>>>
{
    public async Task<ResultDTO<List<MealViewDTO>>> Handle(
        GetMealViewAllQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = mealViewRepository.MultipleResultQuery();

            var entities = await mealViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<MealViewDTO>>(entities);

            return ResultDTO<List<MealViewDTO>>.Success(
                dtos,
                "لیست کامل وعده‌های غذایی با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<MealViewDTO>>.Failure(
                $"خطا در دریافت لیست وعده‌های غذایی: {ex.Message}",
                new List<MealViewDTO>()
            );
        }
    }
}
